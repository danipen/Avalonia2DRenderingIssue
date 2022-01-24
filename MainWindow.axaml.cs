using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia2DRenderingIssue
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.Renderer.DrawDirtyRects = true;
            this.Renderer.DrawFps = true;

            DockPanel dockPanel = new DockPanel();
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Spacing = 5;

            mZoomInButton = new Button() { Content = "Zoom In" };
            mZoomOutButton = new Button() { Content = "Zoom Out" };

            mZoomInButton.Click += ZoomInButton_Click;
            mZoomOutButton.Click += ZoomOutButton_Click;

            stackPanel.Children.Add(mZoomInButton);
            stackPanel.Children.Add(mZoomOutButton);

            DockPanel.SetDock(stackPanel, Dock.Top);

            mCanvas = new Canvas();
            mCanvas.Width = mCanvasWidth * mZoomLevel;
            mCanvas.RenderTransform = new ScaleTransform(mZoomLevel, mZoomLevel);
            mCanvas.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);

            mScrollViewer = new ScrollViewer();
            mScrollViewer.Content = mCanvas;
            mScrollViewer.VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            mScrollViewer.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;

            mScrollViewer.ClipToBounds = true;
            mCanvas.ClipToBounds = true;

            DockPanel contentPanel = new DockPanel();
            
            Panel leftPanel = new Panel();
            leftPanel.Background = Brushes.LightGray;
            leftPanel.Width = 300;
            DockPanel.SetDock(leftPanel, Dock.Left);

            Panel rightPanel = new Panel();
            rightPanel.Background = Brushes.LightGray;
            rightPanel.Width = 300;
            DockPanel.SetDock(rightPanel, Dock.Right);

            contentPanel.Children.Add(leftPanel);
            contentPanel.Children.Add(rightPanel);
            contentPanel.Children.Add(mScrollViewer);

            dockPanel.Children.Add(stackPanel);
            dockPanel.Children.Add(contentPanel);

            Content = dockPanel;

            BranchShape branchShape = new BranchShape();
            mCanvas.Children.Add(branchShape);

            MoveToShape();
        }

        private void ZoomOutButton_Click(object? sender, RoutedEventArgs e)
        {
            SetZoomLevel(mZoomLevel - 0.1);
        }

        private void ZoomInButton_Click(object? sender, RoutedEventArgs e)
        {
            SetZoomLevel(mZoomLevel + 0.1);
        }

        void SetZoomLevel(double zoomLevel)
        {
            mZoomLevel = zoomLevel;
            mCanvas.Width = mCanvasWidth * mZoomLevel;

            ((ScaleTransform)mCanvas.RenderTransform).ScaleX = mZoomLevel;
            ((ScaleTransform)mCanvas.RenderTransform).ScaleY = mZoomLevel;

            MoveToShape();
        }

        void MoveToShape()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                mScrollViewer.Offset = new Vector((1934831 + 1786 - 150) * mZoomLevel, 0)
            , DispatcherPriority.Render - 1);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        double mCanvasWidth = 2004500;
        Button mZoomInButton;
        Button mZoomOutButton;
        Canvas mCanvas;
        ScrollViewer mScrollViewer;
        double mZoomLevel = 1.5;
    }
}