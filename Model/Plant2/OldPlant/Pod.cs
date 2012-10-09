﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSGeneral;
using ModelFramework;

public class Pod : Organ1, AboveGround
{
    #region Parameters read from XML file and links to other functions.
    [Link]
    public Component My;

    [Link]
    Environment Environment = null;

    [Link]
    Function TE = null;

    [Link]
    Grain Grain = null;

    [Link]
    Function FractionGrainInPod = null;

    [Link]
    Function NConcentrationCritical = null;

    [Link]
    Function NConcentrationMinimum = null;

    [Link]
    Function NConcentrationMaximum = null;

    [Link]
    Plant15 Plant = null;

    [Link]
    CompositeBiomass TotalGreen = null;

    [Link]
    Function GrowthStructuralFractionStage = null;

    [Link]
    Function DMSenescenceFraction = null;

    [Link]
    Population1 Population = null;

    [Param]
    double InitialWt = 0;

    [Param]
    double InitialNConcentration = 0;

    [Param]
    double NDeficitUptakeFraction = 1.0;

    [Param]
    double NSenescenceConcentration = 0;

    [Param]
    double SenescenceDetachmentFraction = 0;

    #endregion

    #region Private variables
    private double dlt_dm_pot_rue;
    private double dlt_n_senesced_retrans;           // plant N retranslocated to/from (+/-) senesced part to/from <<somewhere else??>> (g/m^2)
    private double dlt_n_senesced_trans;
    private Biomass GreenRemoved = new Biomass();
    private Biomass SenescedRemoved = new Biomass();
    private double _DMGreenDemand;
    private double _NDemand;
    private double _SoilNDemand;
    private double NMax;
    private double sw_demand_te;
    private double sw_demand;
    private double n_conc_crit = 0;
    private double n_conc_max = 0;
    private double n_conc_min = 0;
    private double transpEff;
    #endregion

    #region Public interface defined by Organ1
    public string Name { get { return My.Name; } }
    public Biomass Green { get; private set; }
    public Biomass Senesced { get; private set; }
    public Biomass Senescing { get; private set; }
    public Biomass Retranslocation { get; private set; }
    public Biomass Growth { get; private set; }
    public Biomass Detaching { get; private set; }

    // Soil water
    public double SWSupply { get { return 0; } }
    public double SWDemand { get { return sw_demand; } }
    public double SWUptake { get { return 0; } }
    public void DoSWDemand(double Supply)
    {
        // Return crop water demand from soil by the crop (mm) calculated by
        // dividing biomass production limited by radiation by transpiration efficiency.
        // get potential transpiration from potential
        // carbohydrate production and transpiration efficiency

        // Calculate today's transpiration efficiency from min,max temperatures and co2 level
        // and converting mm water to g dry matter (g dm/m^2/mm water)

        transpEff = TE.Value / Environment.VPD / Conversions.g2mm;

        if (transpEff == 0)
        {
            sw_demand_te = 0;
            sw_demand = 0;
        }
        else
        {
            sw_demand_te = dlt_dm_pot_rue / transpEff;

            // For some reason, the old PLANT module used a CoverGreen of zero in this calculation
            // and not the calculated one from the DoCover method. Is this a bug? - DPH
            double ZeroCoverGreen = 0;

            // Capping of sw demand will create an effective TE- recalculate it here
            // In an ideal world this should NOT be changed here - NIH
            double SWDemandMax = Supply * ZeroCoverGreen;
            sw_demand = MathUtility.Constrain(sw_demand_te, Double.MinValue, SWDemandMax);
            transpEff = transpEff * MathUtility.Divide(sw_demand_te, sw_demand, 1.0);
        }
        Util.Debug("Pod.sw_demand=%f", sw_demand);
        Util.Debug("Pod.transpEff=%f", transpEff);
    }
    public void DoSWUptake(double SWDemand) { }
    
    // dry matter
    public double DMSupply { get { return 0.0; } }
    public double DMRetransSupply { get { return 0; } }
    public double dltDmPotRue { get { return 0; } }
    public double DMGreenDemand { get { return _DMGreenDemand; } }
    public double DMDemandDifferential
    {
        get
        {
            return MathUtility.Constrain(DMGreenDemand - Growth.Wt, 0.0, Double.MaxValue);
        }
    }
    public void DoDMDemand(double DMSupply)
    {
        Grain.doProcessBioDemand();

        double dlt_dm_supply_by_pod = 0.0;  // FIXME
        DMSupply += dlt_dm_supply_by_pod;

        double dm_grain_demand = Grain.DltDmPotentialGrain;

        if (dm_grain_demand > 0.0)
            _DMGreenDemand = dm_grain_demand * FractionGrainInPod.Value - dlt_dm_supply_by_pod;
        else
            _DMGreenDemand = DMSupply * FractionGrainInPod.Value - dlt_dm_supply_by_pod;
        Util.Debug("Pod.DMGreenDemand=%f", _DMGreenDemand);
    }
    public void DoDmRetranslocate(double DMAvail, double DMDemandDifferentialTotal)
    {
        Retranslocation.NonStructuralWt += DMAvail * MathUtility.Divide(DMDemandDifferential, DMDemandDifferentialTotal, 0.0);
        Util.Debug("pod.Retranslocation=%f", Retranslocation.NonStructuralWt);
    }
    public void GiveDmGreen(double Delta)
    {
        Growth.StructuralWt += Delta * GrowthStructuralFractionStage.Value;
        Growth.NonStructuralWt += Delta * (1.0 - GrowthStructuralFractionStage.Value);
        Util.Debug("Pod.Growth.StructuralWt=%f", Growth.StructuralWt);
        Util.Debug("Pod.Growth.NonStructuralWt=%f", Growth.NonStructuralWt);
    }
    public void DoSenescence()
    {
        double fraction_senescing = MathUtility.Constrain(DMSenescenceFraction.Value, 0.0, 1.0);

        Senescing.StructuralWt = (Green.StructuralWt + Growth.StructuralWt + Retranslocation.StructuralWt) * fraction_senescing;
        Senescing.NonStructuralWt = (Green.NonStructuralWt + Growth.NonStructuralWt + Retranslocation.NonStructuralWt) * fraction_senescing;
        Util.Debug("Pod.Senescing.StructuralWt=%f", Senescing.StructuralWt);
        Util.Debug("Pod.Senescing.NonStructuralWt=%f", Senescing.NonStructuralWt);
    }
    public void DoDetachment()
    {
        Detaching = Senesced * SenescenceDetachmentFraction;
        Util.Debug("Pod.Detaching.Wt=%f", Detaching.Wt);
        Util.Debug("Pod.Detaching.N=%f", Detaching.N);
    }

    // nitrogen
    public double NDemand { get { return _NDemand; } }
    public double NSupply { get { return 0; } }
    public double NUptake { get { return 0; } }
    public double SoilNDemand { get { return _SoilNDemand; } }
    public double NCapacity
    {
        get
        {
            return MathUtility.Constrain(NMax - NDemand, 0.0, double.MaxValue);
        }
    }
    public double NDemandDifferential { get { return MathUtility.Constrain(NDemand - Growth.N, 0.0, double.MaxValue); } }
    public double AvailableRetranslocateN
    {
        get
        {
            double N_min = n_conc_min * Green.Wt;
            double N_avail = MathUtility.Constrain(Green.N - N_min, 0.0, double.MaxValue);
            double n_retrans_fraction = 1.0;
            return (N_avail * n_retrans_fraction);
        }
    }
    public double DltNSenescedRetrans { get { return dlt_n_senesced_retrans; } }
    public void DoNDemand(bool IncludeRetransloation)
    {

        double TopsDMSupply = 0;
        double TopsDltDmPotRue = 0;
        foreach (Organ1 Organ in Plant.Tops)
        {
            TopsDMSupply += Organ.DMSupply;
            TopsDltDmPotRue += Organ.dltDmPotRue;
        }

        if (IncludeRetransloation)
            Util.CalcNDemand(TopsDMSupply, TopsDltDmPotRue, n_conc_crit, n_conc_max, Growth, Green, Retranslocation.N, NDeficitUptakeFraction,
                      ref _NDemand, ref NMax);
        else
            Util.CalcNDemand(TopsDMSupply, TopsDltDmPotRue, n_conc_crit, n_conc_max, Growth, Green, 0.0, NDeficitUptakeFraction,
                      ref _NDemand, ref NMax);
        Util.Debug("Pod.NDemand=%f", _NDemand);
        Util.Debug("Pod.NMax=%f", NMax);
    }
    public void DoNDemand1Pot(double dltDmPotRue)
    {
        Biomass OldGrowth = Growth;
        Growth.StructuralWt = dltDmPotRue * MathUtility.Divide(Green.Wt, TotalGreen.Wt, 0.0);
        Util.Debug("Pod.Growth.StructuralWt=%f", Growth.StructuralWt);

        Util.CalcNDemand(dltDmPotRue, dltDmPotRue, n_conc_crit, n_conc_max, Growth, Green, Retranslocation.N, 1.0,
                   ref _NDemand, ref NMax);
        Growth.StructuralWt = 0.0;
        Growth.NonStructuralWt = 0.0;
        Util.Debug("Pod.NDemand=%f", _NDemand);
        Util.Debug("Pod.NMax=%f", NMax);
    }
    public void DoSoilNDemand()
    {
        _SoilNDemand = NDemand - dlt_n_senesced_retrans;
        _SoilNDemand = MathUtility.Constrain(_SoilNDemand, 0.0, double.MaxValue);
        Util.Debug("Pod.SoilNDemand=%f", _SoilNDemand);
    }
    public void DoNSupply() { }
    public void DoNRetranslocate(double NSupply, double GrainNDemand)
    {
        if (GrainNDemand >= NSupply)
        {
            // demand greater than or equal to supply
            // retranslocate all available N
            Retranslocation.StructuralN = -AvailableRetranslocateN;
        }
        else
        {
            // supply greater than demand.
            // Retranslocate what is needed
            Retranslocation.StructuralN = -GrainNDemand * MathUtility.Divide(AvailableRetranslocateN, NSupply, 0.0);
        }
        Util.Debug("Pod.Retranslocation.N=%f", Retranslocation.N);
    }
    public void DoNSenescence()
    {
        double green_n_conc = MathUtility.Divide(Green.N, Green.Wt, 0.0);
        double dlt_n_in_senescing_part = Senescing.Wt * green_n_conc;
        double sen_n_conc = Math.Min(NSenescenceConcentration, green_n_conc);

        double SenescingN = Senescing.Wt * sen_n_conc;
        Senescing.StructuralN = MathUtility.Constrain(SenescingN, double.MinValue, Green.N);

        dlt_n_senesced_trans = dlt_n_in_senescing_part - Senescing.N;
        dlt_n_senesced_trans = MathUtility.Constrain(dlt_n_senesced_trans, 0.0, double.MaxValue);

        Util.Debug("Pod.SenescingN=%f", SenescingN);
        Util.Debug("Pod.dlt.n_senesced_trans=%f", dlt_n_senesced_trans);
    }
    public void DoNSenescedRetranslocation(double navail, double n_demand_tot)
    {
        dlt_n_senesced_retrans = navail * MathUtility.Divide(NDemand, n_demand_tot, 0.0);
        Util.Debug("Pod.dlt.n_senesced_retrans=%f", dlt_n_senesced_retrans);
    }
    public void DoNPartition(double GrowthN)
    {
        Growth.StructuralN = GrowthN;
    }
    public void DoNFixRetranslocate(double NFixUptake, double nFixDemandTotal)
    {
        Growth.StructuralN += NFixUptake * MathUtility.Divide(NDemandDifferential, nFixDemandTotal, 0.0);
    }
    public void DoNConccentrationLimits()
    {
        n_conc_crit = NConcentrationCritical.Value;
        n_conc_min = NConcentrationMinimum.Value;
        n_conc_max = NConcentrationMaximum.Value;
        Util.Debug("Pod.n_conc_crit=%f", n_conc_crit);
        Util.Debug("Pod.n_conc_min=%f", n_conc_min);
        Util.Debug("Pod.n_conc_max=%f", n_conc_max);
    }
    public void ZeroDltNSenescedTrans()
    {
        dlt_n_senesced_trans = 0;
    }
    public void DoNUptake(double PotNFix) { }

    // cover
    public double CoverGreen { get; private set; }
    public double CoverSen { get { return 0; } }
    public double CoverTotal { get { return 1.0 - (1.0 - CoverGreen) * (1.0 - CoverSen); } }
    public void DoPotentialRUE()
    {
        Util.Debug("Pod.dlt.dm_pot_rue=%f", 0.0);
    }
    public double interceptRadiation(double incomingSolarRadiation) { return 0; }
    public void DoCover()
    {
        Util.Debug("pod.cover.green=%f", 0.0);
    }

    // update
    public void Update()
    {
        Green = Green + Growth - Senescing;

        Senesced = Senesced - Detaching + Senescing;
        Green = Green + Retranslocation;
        Green.StructuralN = Green.N + dlt_n_senesced_retrans;

        Biomass dying = Green * Population.DyingFractionPlants;
        Green = Green - dying;
        Senesced = Senesced + dying;
        Senescing = Senescing + dying;

        Util.Debug("Pod.Green.Wt=%f", Green.Wt);
        Util.Debug("Pod.Green.N=%f", Green.N);
        Util.Debug("Pod.Senesced.Wt=%f", Senesced.Wt);
        Util.Debug("Pod.Senesced.N=%f", Senesced.N);
        Util.Debug("Pod.Senescing.Wt=%f", Senescing.Wt);
        Util.Debug("Pod.Senescing.N=%f", Senescing.N);
        Util.Debug("pod.partAI=%f", 0.0);
    }
    #endregion

    #region Public interface specific to Pod
    internal void CalcDltPodArea()
    {
        // The old PLANT model always had a dltDm of zero which results in a dlt_partAI and PartAI of zero.
        // This in turn results in CoverGreen always = 0
        // Is this a bug? - DPH
        Util.Debug("Pod.dlt_partAI=%f", 0.0);
    }
    #endregion

    #region Event handlers
    [EventHandler]
    public void OnInitialised()
    {
        Green = new Biomass();
        Senesced = new Biomass();
        Growth = new Biomass();
        Senescing = new Biomass();
        Detaching = new Biomass();
        Retranslocation = new Biomass();
    }
    public  void OnPrepare()
    {
        Growth.Clear();
        Senescing.Clear();
        Detaching.Clear();
        Retranslocation.Clear();
        GreenRemoved.Clear();
        SenescedRemoved.Clear();

        dlt_dm_pot_rue = 0.0;
        dlt_n_senesced_retrans = 0.0;
        dlt_n_senesced_trans = 0.0;

        _DMGreenDemand = 0.0;
        _NDemand = 0.0;
        _SoilNDemand = 0.0;
        NMax = 0.0;
        sw_demand_te = 0.0;
        sw_demand = 0.0;
    }
    public  void OnHarvest(HarvestType Harvest, BiomassRemovedType BiomassRemoved)
    {
        double dm_init = MathUtility.Constrain(InitialWt * Population.Density, double.MinValue, Green.Wt);
        double n_init = MathUtility.Constrain(dm_init * InitialNConcentration, double.MinValue, Green.N);
        //double p_init = MathUtility.Constrain(dm_init * SimplePart::c.p_init_conc, double.MinValue, Green.P);

        double retain_fr_green = MathUtility.Divide(dm_init, Green.Wt, 0.0);
        double retain_fr_sen = 0.0;

        double dlt_dm_harvest = Green.Wt + Senesced.Wt - dm_init;
        double dlt_n_harvest = Green.N + Senesced.N - n_init;
        //double dlt_p_harvest = Green.P + Senesced.P - p_init;

        Senesced = Senesced * retain_fr_sen;
        Green.StructuralWt = Green.Wt * retain_fr_green;
        Green.StructuralN = n_init;
        //Green.P = p_init;

        int i = Util.IncreaseSizeOfBiomassRemoved(BiomassRemoved);
        BiomassRemoved.dm_type[i] = Name;
        BiomassRemoved.fraction_to_residue[i] = (float)(1.0 - Harvest.Remove);
        BiomassRemoved.dlt_crop_dm[i] = (float)(dlt_dm_harvest * Conversions.gm2kg / Conversions.sm2ha);
        BiomassRemoved.dlt_dm_n[i] = (float)(dlt_n_harvest * Conversions.gm2kg / Conversions.sm2ha);
        //BiomassRemoved.dlt_dm_p[i] = (float)(dlt_p_harvest * Conversions.gm2kg / Conversions.sm2ha);
    }
    public  void OnEndCrop(BiomassRemovedType BiomassRemoved)
    {
        int i = Util.IncreaseSizeOfBiomassRemoved(BiomassRemoved);
        BiomassRemoved.dm_type[i] = Name;
        BiomassRemoved.fraction_to_residue[i] = 1.0F;
        BiomassRemoved.dlt_crop_dm[i] = (float)((Green.Wt + Senesced.Wt) * Conversions.gm2kg / Conversions.sm2ha);
        BiomassRemoved.dlt_dm_n[i] = (float)((Green.N + Senesced.N) * Conversions.gm2kg / Conversions.sm2ha);
        //BiomassRemoved.dlt_dm_p[i] = (float)((Green.P + Senesced.P) * Conversions.gm2kg / Conversions.sm2ha);

        Senesced.Clear();
        Green.Clear();
    }
    [EventHandler]
    public void OnPhaseChanged(PhaseChangedType PhenologyChange)
    {
        if (PhenologyChange.NewPhaseName == "EmergenceToEndOfJuvenile")
        {
            Green.StructuralWt = InitialWt * Population.Density;
            Green.StructuralN = InitialNConcentration * Green.StructuralWt;
            Util.Debug("Pod.InitGreen.StructuralWt=%f", Green.StructuralWt);
            Util.Debug("Pod.InitGreen.StructuralN=%f", Green.StructuralN);
        }
    }
    #endregion

}


