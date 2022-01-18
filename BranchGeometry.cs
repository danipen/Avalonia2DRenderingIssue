using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Avalonia2DRenderingIssue
{
    public static class BrExDrawProperties
    {
        public const int ChangesetRadius = 18;
        public const int ChangesetDrawingHeight = ChangesetRadius * 2;
        public const int BranchHeight = 18;
    } 

     public class SubBranchContainerDrawInfo
    {
        public Rect Bounds;

        public int Depth;
    }

    static class BranchGeometry
        {
            const double BRANCH_RADIUS = 12;

            internal static StreamGeometry Create(
                Rect main,
                SubBranchContainerDrawInfo[] subrectangles)
            {
                StreamGeometry geometry = new StreamGeometry();

                using (var context = new PreciseStreamGeometryContextWrapper(geometry.Open()))
                {
                    Create(
                        context,
                        main.X, main.Y, main.Width, main.Height,
                        subrectangles);
                }

                return geometry;
            }

            public static void Create(
                PreciseStreamGeometryContextWrapper context,
                double x,
                double y,
                double width,
                double height,
                SubBranchContainerDrawInfo[] containers)
            {
                int shapeOffset =
                    (BrExDrawProperties.ChangesetDrawingHeight -
                     BrExDrawProperties.BranchHeight) / 2;

                y += shapeOffset;
                height -= shapeOffset * 2;

                List<Rect> subRectangles = GetSubRectangles(containers);
                List<Rect> rectangles = CalculateFusions(subRectangles);

                Rect branchRectangle = new Rect(x, y, width, height);
                AdjustLastRectangle(rectangles, branchRectangle);
                AdjustFirstRectangle(rectangles, branchRectangle);

                double radius = height / 2;
                double xw = x + width;
                double yh = y + height;
                double xwr = xw - radius;
                double xr = x + radius;
                double r2 = radius * 2;
                double xwr2 = xw - r2;

                double angle = 180;
                Point rightArcTargetPoint = new Point(xwr, yh);

                if (rectangles.Count > 0)
                {
                    Rect last = (Rect)rectangles[rectangles.Count - 1];
                    if (last.Right >= xwr2 + r2)
                    {
                        angle = 90;
                        rightArcTargetPoint = new Point(xw, y + radius);
                    }
                }

                context.BeginFigure(new Point(xr, y), true);

                //Top Edge
                context.LineTo(new Point(xwr, y));

                // Right arc
                context.ArcTo(rightArcTargetPoint, new Size(radius, radius), angle, false, SweepDirection.Clockwise);

                //Bottom Edge
                double currentx = rightArcTargetPoint.X;
                double currenty = rightArcTargetPoint.Y;

                if (rectangles != null)
                {
                    for (int i = rectangles.Count - 1; i >= 0; i--)
                    {
                        Rect currentRectangle = (Rect)rectangles[i];
                        Point end = AddBottomRectangle(context, currentRectangle, currentx, currenty, x, shapeOffset);
                        currentx = end.X;
                        currenty = currentRectangle.Top + shapeOffset;
                    }
                }

                float angle2 = 180;

                if (rectangles.Count > 0 && rectangles[0].Left <= x)
                {
                    angle2 = 90;
                    context.LineTo(new Point(rectangles[0].Left, y + radius));
                }
                else
                {
                    context.LineTo(new Point(xr, yh));
                }

                // Left arc
                context.ArcTo(new Point(xr, y), new Size(radius, radius), angle2, false, SweepDirection.Clockwise);
                context.EndFigure(true);
            }

            // Add a rectangle in the botton, from right to left
            static Point AddBottomRectangle(PreciseStreamGeometryContextWrapper context, Rect r, double x, double y, double minx, double shapeOffset)
            {
                r = new Rect(
                    r.X, r.Y + shapeOffset, r.Width, r.Height);

                double xTopLeft = r.Left - BRANCH_RADIUS;
                double xTopRight = r.Right + BRANCH_RADIUS;
                double xBottomLeft = r.Left + BRANCH_RADIUS;
                double xBottomRight = r.Right - BRANCH_RADIUS;

                double yTop = r.Top + BRANCH_RADIUS;
                double yBottom = r.Bottom - BRANCH_RADIUS;

                if (x > r.Right)
                {
                    context.LineTo(new Point(xTopRight, r.Top));
                    context.ArcTo(new Point(r.Right, yTop), new Size(BRANCH_RADIUS, BRANCH_RADIUS), 90, false, SweepDirection.CounterClockwise);
                }

                context.LineTo(new Point(r.Right, yBottom));
                context.ArcTo(new Point(xBottomRight, r.Bottom), new Size(BRANCH_RADIUS, BRANCH_RADIUS), 90, false, SweepDirection.Clockwise);
                context.LineTo(new Point(xBottomLeft, r.Bottom));
                context.ArcTo(new Point(r.Left, yBottom), new Size(BRANCH_RADIUS, BRANCH_RADIUS), 90, false, SweepDirection.Clockwise);
                context.LineTo(new Point(r.Left, yTop));

                if (r.Left > minx)
                {
                    context.ArcTo(new Point(xTopLeft, r.Top), new Size(BRANCH_RADIUS, BRANCH_RADIUS), 90, false, SweepDirection.CounterClockwise);
                }
                else
                {
                    context.LineTo(new Point(r.Left, r.Top));
                }

                // return where the path ends
                return new Point(xTopLeft, r.Top);
            }

            static List<Rect> CalculateFusions(List<Rect> rectangles)
            {
                if (rectangles.Count < 2)
                    return rectangles;

                List<Rect> result = new List<Rect>();

                for (int i = 0; i < rectangles.Count - 1; i++)
                {
                    Rect current = (Rect)rectangles[i];
                    Rect next = (Rect)rectangles[i + 1];

                    if (current.Right + (2 * BRANCH_RADIUS) >= next.Left)
                    {
                        return CalculateFusions(Fusion(rectangles, current, next, i, i + 1));
                    }

                    result.Add(current);
                }

                result.Add(rectangles[rectangles.Count - 1]);

                return result;
            }

            static List<Rect> Fusion(
                List<Rect> rectangles,
                Rect current,
                Rect next,
                int index1,
                int index2)
            {
                List<Rect> result = new List<Rect>();

                Rect fusioned = new Rect(
                    current.X,
                    current.Y,
                    current.Width + 2 * BRANCH_RADIUS + next.Width,
                    (int)Math.Max(current.Height, next.Height));

                for (int i = 0; i < rectangles.Count; i++)
                {
                    if (i == index2) continue;

                    if (i == index1)
                    {
                        result.Add(fusioned);
                    }
                    else
                    {
                        result.Add(rectangles[i]);
                    }
                }

                return result;
            }

            static void AdjustFirstRectangle(List<Rect> rectangles, Rect branchRectangle)
            {
                if (rectangles.Count == 0)
                    return;

                Rect first = rectangles[0];

                if (branchRectangle.X + (3 * BRANCH_RADIUS) > first.X)
                {
                    Rect newFirst = new Rect(
                        branchRectangle.X, first.Y,
                        first.Width + (first.X - branchRectangle.X),
                        first.Height);

                    rectangles[0] = newFirst;
                }
            }

            static void AdjustLastRectangle(List<Rect> rectangles, Rect branchRectangle)
            {
                if (rectangles.Count == 0)
                    return;

                Rect last = rectangles[rectangles.Count - 1];

                if (last.X + last.Width + (3 * BRANCH_RADIUS) >= branchRectangle.X + branchRectangle.Width)
                {
                    Rect newLast = new Rect(
                        last.X, last.Y,
                        branchRectangle.Right - last.X,
                        last.Height);

                    rectangles[rectangles.Count - 1] = newLast;
                }
            }

            static List<Rect> GetSubRectangles(SubBranchContainerDrawInfo[] containers)
            {
                List<Rect> result = new List<Rect>();

                if (containers == null)
                    return result;

                foreach (SubBranchContainerDrawInfo container in containers)
                {
                    result.Add(container.Bounds);
                }

                return result;
            }
        }
}