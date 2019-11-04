﻿namespace Mikodev.Binary.Creators.Values
{
    internal sealed class DecimalAdapter : Adapter<decimal, (int, int, int, int)>
    {
        public override (int, int, int, int) Of(decimal item)
        {
            var bits = decimal.GetBits(item);
            var last = bits[3];
            return (bits[0], bits[1], bits[2], last);
        }

        public override decimal To((int, int, int, int) item)
        {
            var (head, init, tail, last) = item;
            var bits = new[] { head, init, tail, last };
            return new decimal(bits);
        }
    }
}