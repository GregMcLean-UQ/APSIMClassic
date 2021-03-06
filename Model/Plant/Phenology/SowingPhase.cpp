#include "StdPlant.h"

#include "SowingPhase.h"

bool SowingPhase::germinating()
   {
   //===========================================================================
   // Determine whether seed germinates based on soil water availability
   // Soil water content of the seeded layer must be > the
   // lower limit to be adequate for germination.

   // plant extractable soil water in seedling layer inadequate for germination (mm/mm)
   float pesw_germ;
   scienceAPI.read("pesw_germ", pesw_germ, 0.0f, 1.0f);

   return (plant.getPeswSeed() > pesw_germ);
   }



void SowingPhase::calcPhaseDevelopment(int das, float& dlt_tt_phenol, float& phase_devel)
   {
   if (DaysFromSowingToEndOfPhase > 0)
      {
      if (das >= DaysFromSowingToEndOfPhase)
         phase_devel = 1.999f;
      else
         phase_devel = 0.999f;
      }
   else
      {
       
      // can't germinate on same day as sowing, because we would miss out on
      // day of sowing elsewhere.
      if (das > 0 && germinating())
         phase_devel = 1.999f;
      else
         phase_devel = 0.999f;
      dlt_tt_phenol = TT();
      }
   }


