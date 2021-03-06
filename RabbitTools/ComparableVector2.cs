﻿using System;

namespace RabbitTools
{
    class ComparableVector2 : IComparable
    {
        public float X, Y;

        public ComparableVector2(float X, float Y) {
            this.X = X;
            this.Y = Y;
        }

        private float Distance(ComparableVector2 v)
        {
            return (float)Math.Sqrt(Math.Pow(v.X, 2) + Math.Pow(v.Y, 2));
        }
        
        public int CompareTo(Object o)
        {
            return (int)(Distance(this) - Distance((ComparableVector2)o));
        }
    }
}
