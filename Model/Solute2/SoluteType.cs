﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using ModelFramework;
using CSGeneral;

/// <summary>
/// Base class for controlling the solute type/name and its outputs
/// </summary>
/// <remarks>
/// This class hold the variables with the values of several outputs, which are updated by the class where the computation takes place
/// No outputs are given here, the actual output methods are implemented in derived classes.
/// This class also contain a function to initialise the variable and arrays (solute name and amount), as well as a method to handle
///  the changes in solute passed by water modules (while these use dlt_$$$ - it should change in the future so that only the event
///   OnSoluteChanged is used.
/// </remarks>
public class SoluteType
{
	#region Parameters and variables

	/// <summary>
	/// The name of the solute
	/// </summary>
	public string Name = "None";

	/// <summary>
	/// The amount of solute in each layer (kg/ha)
	/// </summary>
	public double[] SoluteAmount;

	/// <summary>
	/// The variation in solute amount due to diffusion
	/// </summary>
	public double[] SoluteAdsorbed;

	/// <summary>
	/// The variation in solute amount due to diffusion
	/// </summary>
	public double[] deltaSoluteDiffusion;
	
	/// <summary>
	/// The name of the solute's prodcut following degradation
	/// </summary>
	public string ProductName = "None";

	/// <summary>
	/// THe fraction of solute tranformed in this time-step (0-1)
	/// </summary>
	public double[] fractionSoluteTransformation;

	/// <summary>
	/// The amount of solute degradation in this time-step (kg/ha)
	/// </summary>
	public double[] deltaSoluteTransformed;

	/// <summary>
	/// The values of the inhibition factor (0-1)
	/// </summary>
	public double[] InhibitionEffect;

	#endregion

	#region Initialisation procedure

	/// <summary>
	/// Performs some initalisation tasks, set name and array sizes
	/// </summary>
	/// <param name="ThisSolute">A reference to the solute which this type represents</param>
	public void InitialiseSoluteType(Solute ThisSolute)
	{
		// Set the name of the solute
		SetSoluteName();

		// Initialise and set the initial solute amount
		SoluteAmount = ThisSolute.Amount_kgha();

		// Initilise the solute transfomed arrays
		SoluteAdsorbed = new double[SoluteAmount.Length];
		deltaSoluteDiffusion = new double[SoluteAmount.Length];
		fractionSoluteTransformation = new double[SoluteAmount.Length];
		deltaSoluteTransformed = new double[SoluteAmount.Length];
		InhibitionEffect = new double[SoluteAmount.Length];
	}

	/// <summary>
	/// Simple method to set the name of the solute, to be overriden in derived classes
	/// </summary>
	public virtual void SetSoluteName()
	{ Name = "None"; }

	#endregion

	#region Update values procedure

	/// <summary>
	/// Gets and update the new values for solute amount
	/// </summary>
	/// <param name="Amount">The new values</param>
	public void NewSoluteAmount(double[] Amount)
	{
		for (int Layer = 0; Layer < Amount.Length; Layer++)
			SoluteAmount[Layer] = Amount[Layer];
	}

	/// <summary>
	/// Gets the amount of solute adsorbed onto soil particles (kg/ha)
	/// </summary>
	/// <param name="Amount">The adsorbed amount</param>
	public void AdsorptionAmount(double[] Amount)
	{
		for (int Layer = 0; Layer < Amount.Length; Layer++)
			SoluteAdsorbed[Layer] = Amount[Layer];
	}

	/// <summary>
	/// Gets the variations in solute amount due to diffusion this time-step (kg/ha)
	/// </summary>
	/// <param name="Amount">The diffusion values</param>
	public void NewDiffusionAmount(double[] Amount)
	{
		for (int Layer = 0; Layer < Amount.Length; Layer++)
			deltaSoluteDiffusion[Layer] = Amount[Layer];
	}

	/// <summary>
	/// Gets the values of degradation fraction this time-step (0-1)
	/// </summary>
	/// <param name="Fraction">The degradation fraction values</param>
	public void NewDegradationFraction(double[] Fraction)
	{
		for (int Layer = 0; Layer < Fraction.Length; Layer++)
			fractionSoluteTransformation[Layer] = Fraction[Layer];
	}

	/// <summary>
	/// Gets the amount of solute degradation this time-step (kg/ha)
	/// </summary>
	/// <param name="Amount">The degradation values</param>
	public void NewDegradationAmount(double[] Amount)
	{
		for (int Layer = 0; Layer < Amount.Length; Layer++)
			deltaSoluteTransformed[Layer] = Amount[Layer];
	}

	/// <summary>
	/// Gets the value of the inhibition factor (0-1)
	/// </summary>
	/// <param name="Factor">The inhibition factor values</param>
	public void NewInhibitionEffect(double[] Factor)
	{
		for (int Layer = 0; Layer < Factor.Length; Layer++)
			InhibitionEffect[Layer] = Factor[Layer];
	}

	#endregion

	public virtual void EffectivateSoluteDegradation()
	{}
}

#region Derived solute types

/// <summary>
/// Controls the output for solute: Tracer
/// </summary>
public class Type_tracer : SoluteType
{
	/// <summary>
	/// Set the name of this solute
	/// </summary>
	public override void SetSoluteName()
	{ Name = "Tracer"; }

	#region Procedure to handle set dlt_solute

	/// <summary>
	/// Event used to send changes in solute amount to APSIM
	/// </summary>
	[Event]
	public event SoluteChangedDelegate SoluteChanged;

	[Output("dlt_tracer")]
	[Units("kg/ha")]
	public double[] dlt_tracer
	{
		set
		{
			SoluteChangedType SoluteChanges = new SoluteChangedType();
			SoluteChanges.Sender = "SoilWat-SWIM";
			SoluteChanges.SoluteName = Name;
			SoluteChanges.SoluteUnits = "kg/ha";
			SoluteChanges.DeltaSolute = value;

			SoluteChanged.Invoke(SoluteChanges);
		}
	}

	#endregion

	#region Outputs for this solute

	[Output("tracer")]
	[Units("kg/ha")]
	[Description("amount of Tracer in each soil layer")]
	public double[] tracer
	{ get { return SoluteAmount; } }

	[Output("DegradationFraction_tracer")]
	[Units("%")]
	[Description("fraction of Tracer that has been transformed")]
	private double[] DegradationFraction_tracer
	{ get { return fractionSoluteTransformation; } }

	[Output("Degradation_tracer")]
	[Units("kg/ha")]
	[Description("amount of Tracer that has been transformed")]
	private double[] Degradation_tracer
	{ get { return deltaSoluteTransformed; } }

	#endregion
}

/// <summary>
/// Controls the output for solute: GenericSolute
/// </summary>
public class Type_GenericSolute : SoluteType
{
	/// <summary>
	/// Set the name of this solute
	/// </summary>
	public override void SetSoluteName()
	{ 
		Name = "GenericSolute"; 
		ProductName = "Tracer"; 
	}

	#region Procedure to handle set dlt_solute

	/// <summary>
	/// Event used to send changes in solute amount to APSIM
	/// </summary>
	[Event]
	public event SoluteChangedDelegate SoluteChanged;

	[Output("dlt_genericsolute")]
	[Units("kg/ha")]
	public double[] dlt_genericsolute
	{
		set
		{
			SoluteChangedType SoluteChanges = new SoluteChangedType();
			SoluteChanges.Sender = "SoilWat-SWIM";
			SoluteChanges.SoluteName = Name;
			SoluteChanges.SoluteUnits = "kg/ha";
			SoluteChanges.DeltaSolute = value;

			SoluteChanged.Invoke(SoluteChanges);
		}
	}

	#endregion

	#region Procedure to handle solute degradation

	    /// <summary>
	    /// Make the changes from solute degradation effective . Uses the SoluteChanged event
	    /// </summary>
	    public override void EffectivateSoluteDegradation()
	    {
	        SoluteChangedType SoluteChanges = new SoluteChangedType();

			// First reduce the amount of this solute
	        SoluteChanges.Sender = "SoluteDegradation";
	        SoluteChanges.SoluteName = Name;
	        SoluteChanges.SoluteUnits = "kg/ha";
	        SoluteChanges.DeltaSolute = deltaSoluteTransformed;
	        SoluteChanged.Invoke(SoluteChanges);

			// Now increase the degradation product (assumed also solute)
			SoluteChanges.Sender = "SoluteDegradation";
	        SoluteChanges.SoluteName = ProductName;
	        SoluteChanges.SoluteUnits = "kg/ha";
			SoluteChanges.DeltaSolute = Array.ConvertAll(deltaSoluteTransformed, i => -i);
	        SoluteChanged.Invoke(SoluteChanges);
	    }

	#endregion

	#region Basic outputs for this solute

	[Output("genericsolute")]
	[Units("kg/ha")]
	[Description("amount of GenericSolute in each soil layer")]
	private double[] genericsolute
	{ get { return SoluteAmount; } }

	[Output("adsorbed_genericsolute")]
	[Units("kg/ha")]
	[Description("amount of GenericSolute that is adsorbed")]
	private double[] adsorbed_genericsolute
	{ get { return SoluteAdsorbed; } }

	[Output("diffusion_genericsolute")]
	[Units("kg/ha")]
	[Description("amount of GenericSolute that has been moved by diffusion")]
	private double[] diffusion_genericsolute
	{ get { return deltaSoluteDiffusion; } }

	[Output("degradationFraction_genericsolute")]
	[Units("%")]
	[Description("fraction of GenericSolute that has been transformed")]
	private double[] DegradationFraction_genericsolute
	{ get { return fractionSoluteTransformation; } }

	[Output("degradation_genericsolute")]
	[Units("kg/ha")]
	[Description("amount of GenericSolute that has been transformed")]
	private double[] Degradation_genericsolute
	{ get { return Array.ConvertAll(deltaSoluteTransformed, i => -i); } }

	#endregion
}

/// <summary>
/// Controls the output for solute: dcd
/// </summary>
public class Type_dcd : SoluteType
{
	/// <summary>
	/// Set the name of this solute
	/// </summary>
	public override void SetSoluteName()
	{ Name = "dcd"; }

	#region Procedure to handle set dlt_solute

	/// <summary>
	/// Event used to send changes in solute amount to APSIM
	/// </summary>
	[Event]
	public event SoluteChangedDelegate SoluteChanged;

	[Output("dlt_dcd")]
	[Units("kg/ha")]
	public double[] dlt_dcd
	{
		set
		{
			SoluteChangedType SoluteChanges = new SoluteChangedType();
			SoluteChanges.Sender = "SoilWat-SWIM";
			SoluteChanges.SoluteName = Name;
			SoluteChanges.SoluteUnits = "kg/ha";
			SoluteChanges.DeltaSolute = value;

			SoluteChanged.Invoke(SoluteChanges);
		}
	}

	#endregion

	#region Procedure to handle solute degradation

	/// <summary>
	/// Event used to send changes in solute amount to APSIM
	/// </summary>
	[Event]
	public event NitrogenChangedDelegate NitrogenChanged;

	/// <summary>
	/// Make the changes from solute degradation effective . Uses the SoluteChanged event
	/// </summary>
	public override void EffectivateSoluteDegradation()
	{
		// First reduce the amount of this solute
		SoluteChangedType SoluteChanges = new SoluteChangedType();
		SoluteChanges.Sender = "SoluteDegradation";
		SoluteChanges.SoluteName = Name;
		SoluteChanges.SoluteUnits = "kg/ha";
		SoluteChanges.DeltaSolute = deltaSoluteTransformed;
		SoluteChanged.Invoke(SoluteChanges);

		// Now increase the degradation product (assumed also solute)
		NitrogenChangedType NitrogenChanges = new NitrogenChangedType();
		NitrogenChanges.Sender = "SoluteDegradation";
        NitrogenChanges.SenderType = "Solute";
		NitrogenChanges.DeltaUrea = Array.ConvertAll(deltaSoluteTransformed, i => -i);
		NitrogenChanged.Invoke(NitrogenChanges);
	}

	#endregion

	#region Basic outputs for this solute

	[Output("dcd")]
	[Units("kg/ha")]
	[Description("amount of dcd in each soil layer")]
	public double[] dcd
	{ get { return SoluteAmount; } }

	[Output("DegradationFraction_dcd")]
	[Units("%")]
	[Description("fraction of dcd that has been transformed")]
	private double[] DegradationFraction_dcd
	{ get { return fractionSoluteTransformation; } }

	[Output("Degradation_dcd")]
	[Units("kg/ha")]
	[Description("amount of dcd that has been transformed")]
	private double[] Degradation_dcd
	{ get { return Array.ConvertAll(deltaSoluteTransformed, i => -i); } }

	[Output("nitrification_inhibition")]
	[Units("0-1")]
	[Description("the nitrification inhibition factor")]
	private double[] nitrification_inhibition
	{ get { return InhibitionEffect; } }

	#endregion
}

#endregion
