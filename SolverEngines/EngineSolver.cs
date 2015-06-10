﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using KSP;


namespace SolverEngines
{
    public class EngineSolver
    {
        //freestream flight conditions; static pressure, static temperature, static density, and mach number
        public double alt, p0, t0, eair0, vel, M0 = 0, rho, mach;
        public bool oxygen = false;

        //total conditions behind inlet
        public double P1, T1, Rho1;

        // engine state
        public bool running = false;
        public double ffFraction = 1d;

        //gas properties at start
        public double gamma_c, inv_gamma_c, inv_gamma_cm1;
        public double R_c;
        public double Cp_c;
        public double Cv_c;

        // current throttle state
        public double throttle;

        //thrust and Isp and fuel flow of the engine
        public double thrust, Isp, fuelFlow;

        // the string to report when can't thrust
        public string statusString = "";

        public string debugstring;
        //---------------------------------------------------------
        //Initialization Functions

        /// <summary>
        /// Sets the freestream properties
        /// </summary>
        /// <param name="altitude">altitude in m</param>
        /// <param name="pressure">pressure in kPa</param>
        /// <param name="temperature">temperature in K</param>
        /// <param name="velocity">velocity in m/s</param>
        /// <param name="hasOxygen">does the atmosphere contain oxygen</param>
        virtual public void SetFreestreamAndInlet(EngineThermodynamics ambientTherm, EngineThermodynamics inletTherm, double altitude, double inMach, double velocity, bool hasOxygen)
        {
            alt = altitude;
            p0 = ambientTherm.P;
            t0 = ambientTherm.T;
            rho = ambientTherm.Rho;
            oxygen = hasOxygen;
            vel = velocity;
            mach = inMach;

            P1 = inletTherm.P;
            T1 = inletTherm.T;
            Rho1 = inletTherm.Rho;

            gamma_c = inletTherm.Gamma;
            inv_gamma_c = 1d / gamma_c;
            inv_gamma_cm1 = 1d / (gamma_c - 1d);
            Cp_c = inletTherm.Cp;
            Cv_c = inletTherm.Cv;
            R_c = inletTherm.R;


            M0 = vel / Math.Sqrt(gamma_c * R_c * t0);

            eair0 = Math.Sqrt(gamma_c / R_c / t0);
        }

        /// <summary>
        /// Sets the engine state based on module setting and resource availability
        /// </summary>
        /// <param name="isRunning">is the engine running (i.e. active/enabled)</param>
        /// <param name="ffFrac">fraction of desired fuel flow passed to the engine last tick</param>
        virtual public void SetEngineState(bool isRunning, double ffFrac)
        {
            running = isRunning;
            ffFraction = ffFrac;
        }

        /// <summary>
        /// Calculates enigne state based on existing and passed info
        /// </summary>
        /// <param name="airRatio">ratio of air requirement met</param>
        /// <param name="commandedThrottle">current throttle state</param>
        /// <param name="flowMult">a multiplier to fuel flow (and thus thrust)--Isp unchanged</param>
        /// <param name="ispMult">a multiplier to Isp (and thus thrust)--fuel flow unchanged</param>
        virtual public void CalculatePerformance(double airRatio, double commandedThrottle, double flowMult, double ispMult)
        {

            fuelFlow = 0d;
            Isp = 0d;
            thrust = 0d;
            throttle = commandedThrottle;
        }

        // getters for base fields
        public double GetThrust() { return thrust; }
        public double GetIsp() { return Isp; }
        public double GetFuelFlow() { return fuelFlow; }
        public double GetM0() { return M0; }

        // virtual getters
        // Status
        virtual public double GetEngineTemp() { return 288.15d; }
        virtual public double GetArea() { return 0d; }
        virtual public string GetStatus() { return statusString; }
        virtual public bool GetRunning() { return running; }
        // FX
        virtual public double GetEmissive() { return 0d; }
        virtual public float GetFXPower() { return running && ffFraction > 0d ? (float)throttle : 0f; }
        virtual public float GetFXRunning() { return running && ffFraction > 0d ? (float)throttle : 0f; }
        virtual public float GetFXThrottle() { return running && ffFraction > 0d ? (float)throttle : 0f; }
        virtual public float GetFXSpool() { return running && ffFraction > 0d ? (float)throttle : 0f; }


        protected double CalculateGamma(double temperature, double fuel_fraction)
        {
            double gamma = 1.4 - 0.1 * Math.Max((temperature - 300) * 0.0005, 0) * (1 + fuel_fraction);
            gamma = Math.Min(1.4, gamma);
            gamma = Math.Max(1.1, gamma);
            return gamma;
        }

        protected double CalculateCp(double temperature, double fuel_fraction)
        {
            double Cp = 1004.5 + 250 * Math.Max((temperature - 300) * 0.0005, 0) * (1 + 10 * fuel_fraction);
            Cp = Math.Min(1404.5, Cp);
            Cp = Math.Max(1004.5, Cp);
            return Cp;
        }

    }
}