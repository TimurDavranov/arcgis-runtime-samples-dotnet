// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.Buffer
{
    [Register("Buffer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "GeometryEngine",
        "This sample demonstrates how to use GeometryEngine to create planar and geodesic buffer polygons from a map location and buffer distance. It illustrates the difference between planar and geodesic results.",
        "1. Tap on the map.\n2. A planar and a geodesic buffer will be created at the tap location using the distance (miles) specified in the text box.\n3. Continue tapping to create additional buffers. Notice that buffers closer to the equator are similar in size. As you move north or south from the equator, however, the geodesic polygons appear larger. Geodesic polygons are in fact a better representation of the true shape and size of the buffer.\n 4. Click `Clear` to remove all buffers and start again.",
        "Buffer, Geodesic, Planar")]
    public class Buffer : UIViewController
    {
        // Create a map view control to display the map and buffers.
        private MapView _myMapView = new MapView();

        // Instruction label to describe how to use the sample. 
        private UILabel _helpLabel;

        // A toolbar, label, and text view to enter a buffer value (in miles).
        private UIToolbar _bufferInputArea;
        private UILabel _bufferInputLabel;
        private UITextField _bufferDistanceMilesTextField;

        // Toolbar and label controls to show the buffer colors in the UI.
        private UIToolbar _legendArea;
        private UILabel _geodesicSwatchLabel;
        private UILabel _planarSwatchLabel;

        // A button to clear buffer graphics from the display.
        private UIButton _clearBuffersButton;

        public Buffer()
        {
            Title = "Buffer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI.
            CreateLayout();

            // Initialize the map and graphics overlays.
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap and add it to the map view.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Handle the MapView's GeoViewTapped event to create buffers.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create a fill symbol for geodesic buffer polygons.            
            Colors geodesicBufferColor = Colors.FromArgb(120, 255, 0, 0);
            SimpleLineSymbol geodesicOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, geodesicBufferColor, 2);
            SimpleFillSymbol geodesicBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, geodesicBufferColor, geodesicOutlineSymbol);

            // Create a fill symbol for planar buffer polygons.            
            Colors planarBufferColor = Colors.FromArgb(120, 0, 0, 255);
            SimpleLineSymbol planarOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, planarBufferColor, 2);
            SimpleFillSymbol planarBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, planarBufferColor, planarOutlineSymbol);

            // Create a marker symbol for tap locations.
            SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.White, 14);

            // Create a graphics overlay to display geodesic polygons, set its renderer and add it to the map view.
            GraphicsOverlay geodesicPolysOverlay = new GraphicsOverlay
            {
                Id = "GeodesicPolys",
                Renderer = new SimpleRenderer(geodesicBufferFillSymbol)
            };
            _myMapView.GraphicsOverlays.Add(geodesicPolysOverlay);

            // Create a graphics overlay to display planar polygons, set its renderer and add it to the map view.
            GraphicsOverlay planarPolysOverlay = new GraphicsOverlay
            {
                Id = "PlanarPolys",
                Renderer = new SimpleRenderer(planarBufferFillSymbol)
            };
            _myMapView.GraphicsOverlays.Add(planarPolysOverlay);

            // Create a graphics overlay to display tap locations for buffers, set its renderer and add it to the map view.
            GraphicsOverlay tapLocationsOverlay = new GraphicsOverlay
            {
                Id = "TapPoints",
                Renderer = new SimpleRenderer(tapSymbol)
            };
            _myMapView.GraphicsOverlays.Add(tapLocationsOverlay);

            // Show the colors for each type of buffer in the UI.
            ShowBufferSwatches(planarBufferColor, geodesicBufferColor);
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the location tapped by the user (a map point in the WebMercator projected coordinate system).
                MapPoint userTapPoint = e.Location;

                // Get the buffer distance (miles) entered in the text box.
                double bufferInMiles = System.Convert.ToDouble(_bufferDistanceMilesTextField.Text);

                // Call a helper method to convert the input distance to meters.
                double bufferInMeters = LinearUnits.Miles.ToMeters(bufferInMiles);

                // Create a planar buffer graphic around the input location at the specified distance.
                Geometry bufferGeometryPlanar = GeometryEngine.Buffer(userTapPoint, bufferInMeters);
                Graphic planarBufferGraphic = new Graphic(bufferGeometryPlanar);

                // Create a geodesic buffer graphic using the same location and distance.
                Geometry bufferGeometryGeodesic = GeometryEngine.BufferGeodetic(userTapPoint, bufferInMeters, LinearUnits.Meters, double.NaN, GeodeticCurveType.Geodesic);
                Graphic geodesicBufferGraphic = new Graphic(bufferGeometryGeodesic);

                // Create a graphic for the user tap location.
                Graphic locationGraphic = new Graphic(userTapPoint);

                // Get the graphics overlays.
                GraphicsOverlay planarBufferGraphicsOverlay = _myMapView.GraphicsOverlays["PlanarPolys"];
                GraphicsOverlay geodesicBufferGraphicsOverlay = _myMapView.GraphicsOverlays["GeodesicPolys"];
                GraphicsOverlay tapPointGraphicsOverlay = _myMapView.GraphicsOverlays["TapPoints"];

                // Add the buffer polygons and tap location graphics to the appropriate graphic overlays.
                planarBufferGraphicsOverlay.Graphics.Add(planarBufferGraphic);
                geodesicBufferGraphicsOverlay.Graphics.Add(geodesicBufferGraphic);
                tapPointGraphicsOverlay.Graphics.Add(locationGraphic);
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating the buffers.
                UIAlertController alertController = UIAlertController.Create("Error creating buffers", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
                return;
            }
        }

        private void ShowBufferSwatches(Colors planarBufferColor, Colors geodesicBufferColor)
        {
            // Create UIKit.UIColors to represent the System.Drawing.Colors used for the buffers.
            UIColor planarLabelColor = UIColor.FromRGBA(planarBufferColor.R,
                planarBufferColor.G,
                planarBufferColor.B,
                planarBufferColor.A);
            UIColor geodesicLabelColor = UIColor.FromRGBA(geodesicBufferColor.R,
                geodesicBufferColor.G,
                geodesicBufferColor.B,
                geodesicBufferColor.A);

            // Show buffer symbol colors in the UI by setting the appropriate text view fill color.
            _planarSwatchLabel.BackgroundColor = planarLabelColor;
            _geodesicSwatchLabel.BackgroundColor = geodesicLabelColor;
        }

        private void ClearBuffersButton_TouchUpInside(object sender, EventArgs e)
        {
            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in _myMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
            }
        }

        private void CreateLayout()
        {
            // Create the help label.
            _helpLabel = new UILabel
            {
                TextColor = UIColor.Blue,
                Text = "Tap the map to create Planar and Geodesic buffers",
                TextAlignment = UITextAlignment.Center,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0
            };

            // Create the buffer input area (toolbar).
            _bufferInputArea = new UIToolbar();

            // Create the buffer input label.
            _bufferInputLabel = new UILabel
            {
                TextColor = View.TintColor,
                Text = "Distance (miles):",
                TextAlignment = UITextAlignment.Left
            };

            // Create the buffer input control.
            _bufferDistanceMilesTextField = new UITextField
            {
                BackgroundColor = UIColor.White,
                KeyboardType = UIKeyboardType.NumberPad,
                Text = "1000",
                TextAlignment = UITextAlignment.Right,
                TextColor = View.TintColor
            };
            _bufferDistanceMilesTextField.Layer.CornerRadius = 5;
            
            // Allow pressing 'return' to dismiss the keyboard.
            _bufferDistanceMilesTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Create the legend toolbar.
            _legendArea = new UIToolbar();

            // Create the label to show the planar buffer color.
            _planarSwatchLabel = new UILabel
            {
                AdjustsFontSizeToFitWidth = true,
                TextColor = UIColor.White,
                Text = "Planar Buffers",
                TextAlignment = UITextAlignment.Center
            };

            // Create the label to show the geodesic buffer color.
            _geodesicSwatchLabel = new UILabel
            {
                AdjustsFontSizeToFitWidth = true,
                TextColor = UIColor.White,
                Text = "Geodesic Buffers",
                TextAlignment = UITextAlignment.Center
            };

            // Create the clear buffers button.
            _clearBuffersButton = new UIButton
            {
                ClipsToBounds = true,
                BackgroundColor = View.TintColor
            };
            _clearBuffersButton.SetTitle("Clear", UIControlState.Normal);
            _clearBuffersButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _clearBuffersButton.Layer.CornerRadius = 10;

            // Handle the clear buffers button press.
            _clearBuffersButton.TouchUpInside += ClearBuffersButton_TouchUpInside; ;

            // Add views to the page.
            View.AddSubviews(_myMapView,
                _helpLabel,
                _bufferInputLabel,
                _bufferDistanceMilesTextField,
                _legendArea,
                _planarSwatchLabel,
                _geodesicSwatchLabel,
                _clearBuffersButton);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                var topMargin = NavigationController.NavigationBar.Frame.Height +
                                UIApplication.SharedApplication.StatusBarFrame.Height + 10;
                nfloat toolbarHeight = 30;

                // Place the scene view and update the insets to avoid hiding view elements like the attribution bar.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight * 2, 0);

                // Place the help label.
                _helpLabel.Frame = new CGRect(0, topMargin + 10, View.Bounds.Width, toolbarHeight * 2);

                // Place the distance input toolbar.
                _bufferInputArea.Frame = new CGRect(0, View.Bounds.Height - (toolbarHeight * 2), View.Bounds.Width, toolbarHeight);

                // Place the buffer distance input text view and label.
                _bufferInputLabel.Frame = new CGRect(10, View.Bounds.Height - (toolbarHeight * 2) + 5, 150, toolbarHeight - 10);
                _bufferDistanceMilesTextField.Frame = new CGRect(150, View.Bounds.Height - (toolbarHeight * 2) + 5, 50, toolbarHeight - 10);

                // Place the clear buffers button.
                _clearBuffersButton.Frame = new CGRect(220, View.Bounds.Height - (toolbarHeight * 2) + 5, 90, toolbarHeight - 10);

                // Place the legend toolbar.
                _legendArea.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);

                // Place the planar and geodesic legend labels.
                _planarSwatchLabel.Frame = new CGRect(10, View.Bounds.Height - toolbarHeight + 5, 140, toolbarHeight - 10);
                _geodesicSwatchLabel.Frame = new CGRect(160, View.Bounds.Height - toolbarHeight + 5, 140, toolbarHeight - 10);

                base.ViewDidLayoutSubviews();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error laying out sub views: " + ex.Message);
            }
        }
    }
}