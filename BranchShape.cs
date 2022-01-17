using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Avalonia2DRenderingIssue
{
    public class BranchShape : Shape
    {
        protected override Geometry? CreateDefiningGeometry()
        {
            Rect geometryRect = new Rect(
                0, 0,
                2004415,
                36);

            Rect subBranchRect = new Rect(1934831, 18, 1786, 40);

            return BranchGeometry.Create(
                geometryRect,
                new SubBranchContainerDrawInfo[]
                {
                    new SubBranchContainerDrawInfo()
                    {
                        Bounds = subBranchRect,
                        Depth = 1
                    }
                });
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            Pen pen = new Pen(Brushes.DarkBlue, 2);
            context.DrawGeometry(
                Brushes.LightBlue,
                pen,
                DefiningGeometry);
        }
    }
}