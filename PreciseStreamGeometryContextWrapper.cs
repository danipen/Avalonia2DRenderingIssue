using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.RenderHelpers;

namespace Avalonia2DRenderingIssue
{
    public class PreciseStreamGeometryContextWrapper : IGeometryContext, IDisposable
    {
        private readonly StreamGeometryContext _wrapped;
        private Point _currentPoint;

        public PreciseStreamGeometryContextWrapper(StreamGeometryContext context)
        {
            _wrapped = context;
        }

        public void Dispose()
        {
            _wrapped.Dispose();
        }

        public void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection)
        {
            ArcToHelper.ArcTo(_wrapped, _currentPoint, point, size, rotationAngle, isLargeArc, sweepDirection);

            _currentPoint = point;
        }

        public void BeginFigure(Point startPoint, bool isFilled = true)
        {
            _wrapped.BeginFigure(startPoint, isFilled);

            _currentPoint = startPoint;
        }

        public void CubicBezierTo(Point point1, Point point2, Point point3)
        {
            _wrapped.CubicBezierTo(point1, point2, point3);

            _currentPoint = point3;
        }

        public void QuadraticBezierTo(Point control, Point endPoint)
        {
            _wrapped.QuadraticBezierTo(control, endPoint);

            _currentPoint = endPoint;
        }

        public void LineTo(Point point)
        {
            _wrapped.LineTo(point);

            _currentPoint = point;
        }

        public void EndFigure(bool isClosed)
        {
            _wrapped.EndFigure(isClosed);
        }

        public void SetFillRule(FillRule fillRule)
        {
            _wrapped.SetFillRule(fillRule);
        }
    }
}