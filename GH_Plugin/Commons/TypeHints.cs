using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPKG
{
    public static class TypeHints
    {
        static IGH_TypeHint[] PossibleHints =
        {
            new GH_HintSeparator(),
            new GH_BooleanHint_CS(), new GH_IntegerHint_CS(), new GH_DoubleHint_CS(),
            new GH_StringHint_CS(), new GH_DateTimeHint(), new GH_ColorHint(),
            new GH_HintSeparator(),
            new GH_Point3dHint(),
            new GH_Vector3dHint(), new GH_PlaneHint(), new GH_IntervalHint(),
            new GH_UVIntervalHint()
        };
        static readonly List<IGH_TypeHint> g_hints = new List<IGH_TypeHint>();
        public static List<IGH_TypeHint> GetHints()
        {
            lock (g_hints)
            {
                if (g_hints.Count == 0)
                {
                    g_hints.AddRange(PossibleHints);

                    g_hints.Add(new GH_BoxHint());

                    g_hints.Add(new GH_HintSeparator());

                    g_hints.Add(new GH_LineHint());
                    g_hints.Add(new GH_CircleHint());
                    g_hints.Add(new GH_ArcHint());
                    g_hints.Add(new GH_PolylineHint());

                    g_hints.Add(new GH_HintSeparator());

                    g_hints.Add(new GH_CurveHint());
                    g_hints.Add(new GH_MeshHint());
                    g_hints.Add(new GH_SurfaceHint());
                    g_hints.Add(new GH_BrepHint());
                    g_hints.Add(new GH_GeometryBaseHint());
                }
            }
            return g_hints;
        }

    }
}
