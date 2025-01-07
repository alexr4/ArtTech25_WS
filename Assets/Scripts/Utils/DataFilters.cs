using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bonjour.Maths
{
    public class DataFilters
    {
        #region public params
        public enum FilterType {
            Kalman,
            OneEuro
        }

        //kalman
        public KalmanFilterVector3 kalman = new KalmanFilterVector3();
       //Range(0, 1f)] public float Q = 0.000001f;
       //Range(0, 1f)] public float R = 0.01f;

        public OneEuroFilter3 oneEuro = new OneEuroFilter3();
        //[Range(0, 1f)] public float beta = 0.0001f;
        //[Range(0, 1f)] public float minCutOff = 0.04f;
        #endregion

        public Vector3 Filter(Vector3 data, FilterType type, float param1, float param2)
        {
            return type switch
            {
                FilterType.Kalman => kalman.Update(data, param1, param2), //param1 => Q, param2 => R
                FilterType.OneEuro => oneEuro.Update(UnityEngine.Time.time, data, param1, param2), //param1 => Beta, Param2 => minCutOff
                _ => oneEuro.Update(UnityEngine.Time.time, data, param1, param2)
            };
        }
    }
}

