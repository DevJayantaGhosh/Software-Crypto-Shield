using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KeyGenerator.Models
{
    public record CurveName(ECCurve Curve)
    {
        public static CurveName P256 => new(ECCurve.NamedCurves.nistP256);
        public static CurveName P384 => new(ECCurve.NamedCurves.nistP384);
        public static CurveName P521 => new(ECCurve.NamedCurves.nistP521);

        public string Name => Curve.Oid?.FriendlyName ?? Curve.ToString();
    }
}
