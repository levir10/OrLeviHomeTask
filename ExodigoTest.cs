using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.Interop;
using System.Reflection;
[assembly: CommandClass(typeof(OrLeviHomeTask.ExodigoTest))]
namespace OrLeviHomeTask
{
    //define new class containing a duct properties
    public class Duct
    {
        public int Number { get; set; }
        public double Diameter { get; set; }
        public double Distance_from_floor { get; set; }
        public double Distance_from_right { get; set; }
        public bool IsVertical { get; set; }// is the duct drawn on the vertical axis? or horizontal axis? 
        public bool IsPositeve_x { get; set; }// if the duct will show in the positive direction of its axis X>0 
        public bool IsPositeve_y { get; set; }// if the duct will show in the positive direction of its axis Y>0 
        public Duct(int number, double diameter, double distance_from_floor, double distance_from_right, bool isVertical, bool isPositive_x, bool isPositive_y)
        {
            Number = number; Diameter = diameter; Distance_from_floor = distance_from_floor; Distance_from_right = distance_from_right;
            IsVertical = isVertical; IsPositeve_x = isPositive_x; IsPositeve_y = isPositive_y;
        }

    }
    public class Manhole
    {
        public int Id { get; set; }//id of csv row that contain the manhole i
        public string SystemType { get; set; }//system type that runs throgh the manhole
        public double CoverDiameter { get; set; }//system type that runs throgh the manhole
        public string Material { get; set; }//what material the manhole is made of?
        public double Depth { get; set; }//depth of the manhole
        public double Width { get; set; }//manhole section width
        public double Height { get; set; }// manhole section heigh
        public int Azimuth { get; set; }//azimuth of the duct that runs parallel to the duct 1 
        public Manhole(int id, string systemType, double coverDiameter, string material, double depth, double width, double height, int azimuth)
        {
            Id = id; SystemType = systemType; CoverDiameter = coverDiameter; Material = material;
            Depth = depth; Width = width; Height = height; Azimuth = azimuth;
        }

    }
    public class ExodigoTest
    {
        [CommandMethod("DBD")]//DBD==DrawButterflyDiagram
        public void DrawButterflyDiagram()
        {

            // Open file dialog to select CSV file
            string filePath = SelectCsvFile();
            if (string.IsNullOrEmpty(filePath))
            {
                Application.ShowAlertDialog("No file selected.");
                return;
            }

            // Read CSV file
            string[] csvLines = File.ReadAllLines(filePath);

            // Validate CSV format
            if (csvLines.Length < 2)
            {
                Application.ShowAlertDialog("The CSV file is empty or invalid.");
                return;
            }

            // determine delimiter based on the header row
            char delimiter = csvLines[0].Contains('\t') ? '\t' : ',';// for all the different user defined csv

            // Parse CSV header
            string[] headers = csvLines[0].Split(delimiter);
            if (headers.Length < 8)
            {
                Application.ShowAlertDialog("The CSV file does not have the required format.");
                return;
            }


            // Process each row (skipping header)
            double offsetX = 0; // X offset for each plot
            double offsetY = 0; // Y offset for each plot
            double plotSpacing = 50; // Space between plots

            // Start a transaction
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            // Insert image :
            string imageName = "exologo.png";
            string imagePath = Path.Combine(Path.GetTempPath(), imageName);

            // Extract the embedded resource to the temporary path
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("OrLeviHomeTask.exologo.png"))
            {
                if (stream == null) throw new FileNotFoundException("Embedded image not found.");
                using (FileStream fileStream = File.Create(imagePath))
                {
                    stream.CopyTo(fileStream);
                }
            }
            Point3d imagePosition = new Point3d(0, 0, 0); // Position the image at (0, 3, 0)


            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // Open the BlockTable for write
                BlockTable blockTable = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                for (int i = 1; i < csvLines.Length; i++) // Start from the second line
                {

                    string[] row = csvLines[i].Split(delimiter);
                    if (row.Length < 8) continue;
                    // Parse manhole data
                    Manhole manhole = new Manhole(
                        int.Parse(row[0]),
                        row[1],
                        double.Parse(row[2], CultureInfo.InvariantCulture),
                        row[3],
                        double.Parse(row[4], CultureInfo.InvariantCulture),
                        double.Parse(row[5], CultureInfo.InvariantCulture),
                        double.Parse(row[6], CultureInfo.InvariantCulture),
                        int.Parse(row[7])
                    );
                    // Create dimensions for rectangles
                    double verticalWidth = manhole.Width;
                    double verticalHeight = manhole.Depth + manhole.Height + manhole.Depth;
                    double horizontalWidth = manhole.Depth + manhole.Width + manhole.Depth;

                    // Parse ducts
                    Duct[] ducts = new Duct[4];
                    for (int j = 0; j < 4; j++)
                    {
                        int baseIndex = 8 + j * 3;//check  for each j=0,1,2,3==>the spots where each duct parameter-set starts
                        if (baseIndex + 2 < row.Length &&
                            !string.IsNullOrWhiteSpace(row[baseIndex]) &&
                            !string.IsNullOrWhiteSpace(row[baseIndex + 1]) &&
                            !string.IsNullOrWhiteSpace(row[baseIndex + 2]))// if all three parameters has value- dill duct class
                        {
                            ducts[j] = new Duct(
                                j + 1,//number
                                double.Parse(row[baseIndex], CultureInfo.InvariantCulture),//diameter
                                double.Parse(row[baseIndex + 1], CultureInfo.InvariantCulture),//distance from floor
                                double.Parse(row[baseIndex + 2], CultureInfo.InvariantCulture),//distance from right
                                j % 2 == 0, // isVertical property: alternate vertical/horizontal: 0%2==0 is true (vertical), 1%2==0 is false..
                                j % 2 == 0 ?//check if horizontal or vertical
                                (0.5 * manhole.Width - double.Parse(row[baseIndex + 2], CultureInfo.InvariantCulture)) > 0 ://if  vertical- positive_x if ..
                                (j == 1 ? true : false),//if horizontal: right isPositive_x, left is negative
                                j % 2 == 0 ?//check if horizontal or vertical
                                (j == 0 ? true : false) ://if vertical: up is positive down negative
                                (0.5 * manhole.Width - double.Parse(row[baseIndex + 2], CultureInfo.InvariantCulture)) > 0//isPositive_y
                            );
                        }
                    }

                    // new position for this manhole
                    Point3d plotOrigin = new Point3d(offsetX, offsetY, 0);

                    // draw the manhole and ducts
                    DrawManholeAndDucts(modelSpace, trans, manhole, ducts, plotOrigin,imagePath,imagePosition,db);
                    //create new tab layout
                    CreateNewLayoutTab(doc,i, manhole.SystemType, trans, plotOrigin, verticalHeight, horizontalWidth);
                    // update offsets
                    offsetX += plotSpacing;
                    if (i % 5 == 0) // Move to a new row every 5 plots
                    {
                        offsetX = 0;
                        offsetY -= plotSpacing;
                    }


                }

                // Save the drawing
                string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Exodigo_homeTask_or_levi.dwg");
                db.SaveAs(savePath, DwgVersion.Current);


                trans.Commit();
                editor.WriteMessage("Or Levi created the plots successfully.\n");

            }

        }
        //method to open browser and return csv filename
        private string SelectCsvFile()
        {
            // Ensure the project references System.Windows.Forms
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"; // filter to allow CSV files
                dialog.Title = "Select Manhole Data CSV File"; // Set dialog title

                // Show the dialog and check if the user clicked OK
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.FileName; // Return the selected file path
                }
            }
            return null; // put  null if no file was selected
        }
        private void DrawManholeAndDucts(BlockTableRecord modelSpace, Transaction trans, Manhole manhole, Duct[] ducts, Point3d origin,string imagePath, Point3d imagePosition, Database db)
        {
            // Draw rectangles and cover circle
            double verticalWidth = manhole.Width;
            double verticalHeight = manhole.Depth + manhole.Height + manhole.Depth;
            double horizontalWidth = manhole.Depth + manhole.Width + manhole.Depth;

            DrawRectangle(modelSpace, trans, origin, verticalWidth, verticalHeight);
            DrawRectangle(modelSpace, trans, origin, horizontalWidth, manhole.Height);
            
            //manhole cover
            Circle coverCircle = new Circle
            {
                Center = origin,
                Radius = manhole.CoverDiameter / 2
            };
            modelSpace.AppendEntity(coverCircle);
            trans.AddNewlyCreatedDBObject(coverCircle, true);

            //Draw QR Code
            imagePosition = new Point3d(origin.X + manhole.Depth / 2+1,origin.Y -(manhole.Depth / 2+1), 0);
            AddImage(trans, db, imagePath, imagePosition);

            // Draw north arrow (circle and triangle)
            double northArrowRadius = 0.1;
            Point2d northArrowCenter = new Point2d(
               origin.X + Math.Cos(Math.PI / 4) * (manhole.Depth / 2 + northArrowRadius*4), // X-coordinate
               origin.Y + Math.Sin(Math.PI / 4) * (manhole.Depth / 2 + northArrowRadius*4)  // Y-coordinate
            );

            Circle northArrowCircle = new Circle
            {
                Center = new Point3d(northArrowCenter.X, northArrowCenter.Y, 0),
                Radius = northArrowRadius
            };
            modelSpace.AppendEntity(northArrowCircle);
            trans.AddNewlyCreatedDBObject(northArrowCircle, true);

            double azimuthRadians = manhole.Azimuth * (Math.PI / 180);
            Point2d triangleTip = new Point2d(
                northArrowCenter.X + Math.Cos(azimuthRadians) * northArrowRadius,
                northArrowCenter.Y + Math.Sin(azimuthRadians) * northArrowRadius
            );
            Point2d triangleBaseLeft = new Point2d(
                northArrowCenter.X + Math.Cos(azimuthRadians + Math.PI / 2) * northArrowRadius,
                northArrowCenter.Y + Math.Sin(azimuthRadians + Math.PI / 2) * northArrowRadius
            );
            Point2d triangleBaseRight = new Point2d(
                northArrowCenter.X + Math.Cos(azimuthRadians - Math.PI / 2) * northArrowRadius,
                northArrowCenter.Y + Math.Sin(azimuthRadians - Math.PI / 2) * northArrowRadius
            );

            Polyline northArrowTriangle = new Polyline();
            northArrowTriangle.AddVertexAt(0, triangleTip, 0, 0, 0);
            northArrowTriangle.AddVertexAt(1, triangleBaseLeft, 0, 0, 0);
            northArrowTriangle.AddVertexAt(2, triangleBaseRight, 0, 0, 0);
            northArrowTriangle.Closed = true;
            modelSpace.AppendEntity(northArrowTriangle);
            trans.AddNewlyCreatedDBObject(northArrowTriangle, true);

            // Draw ducts
            foreach (var duct in ducts)
            {
                if (duct == null) continue;
                DrawDuct(modelSpace, trans, origin, manhole, duct);
            }
        }

        private void DrawRectangle(BlockTableRecord modelSpace, Transaction trans, Point3d center, double width, double height)
        {
            // Create the rectangle as a Polyline
            Polyline rectangle = new Polyline();
            rectangle.AddVertexAt(0, new Point2d(center.X - width / 2, center.Y - height / 2), 0, 0, 0);
            rectangle.AddVertexAt(1, new Point2d(center.X + width / 2, center.Y - height / 2), 0, 0, 0);
            rectangle.AddVertexAt(2, new Point2d(center.X + width / 2, center.Y + height / 2), 0, 0, 0);
            rectangle.AddVertexAt(3, new Point2d(center.X - width / 2, center.Y + height / 2), 0, 0, 0);
            rectangle.Closed = true;
            modelSpace.AppendEntity(rectangle);
            trans.AddNewlyCreatedDBObject(rectangle, true);

            // Determine the shorter dimension (width or height)
            bool isWidthShorter = width < height;
            double shorterDimension = isWidthShorter ? width : height;

            // Set start and end points for the shorter dimension
            Point3d startPoint, endPoint, dimensionLinePoint;

            if (isWidthShorter)
            {
                // Add dimension for width
                startPoint = new Point3d(center.X - width / 2, center.Y - height / 2, 0);
                endPoint = new Point3d(center.X + width / 2, center.Y - height / 2, 0);
                dimensionLinePoint = new Point3d(center.X, center.Y - height / 2-0.5 , 0); // Dimension line below the rectangle
            }
            else
            {
                // Add dimension for height
                startPoint = new Point3d(center.X + width / 2, center.Y - height / 2, 0);
                endPoint = new Point3d(center.X + width / 2, center.Y + height / 2, 0);
                dimensionLinePoint = new Point3d(center.X + width / 2+0.5, center.Y, 0); // Dimension line to the side of the rectangle
            }

            // Create and add the dimension for the shorter side
            RotatedDimension dimension = new RotatedDimension(
                isWidthShorter ? 0 : Math.PI / 2, // Rotation angle (0 for width, 90 degrees for height)
                startPoint, // Start point of the dimension
                endPoint, // End point of the dimension
                dimensionLinePoint, // Dimension line location
                "", // Dimension text
                ObjectId.Null // No dimension style so that there won't bug it up
            );

            modelSpace.AppendEntity(dimension);
            trans.AddNewlyCreatedDBObject(dimension, true);
        }



        private void DrawDuct(BlockTableRecord modelSpace, Transaction trans, Point3d origin, Manhole manhole, Duct duct)
        {

            double x = 0, y = 0;

            if (duct.IsVertical) // check if the duct is vertical duct (north/south)
            {//check if the duct is on the positive X or not
                x = duct.IsPositeve_x ? manhole.Width / 2 - duct.Distance_from_right - duct.Diameter / 2 : -(manhole.Width / 2 - duct.Distance_from_right - duct.Diameter / 2);
                y = duct.IsPositeve_y ? manhole.Height / 2 + duct.Distance_from_floor + duct.Diameter / 2 : -(manhole.Height / 2 + duct.Distance_from_floor + duct.Diameter / 2);
            }
            else // Horizontal duct (east/west)
            {
                x = duct.IsPositeve_x ? manhole.Width / 2 + duct.Distance_from_floor + duct.Diameter / 2 : -(manhole.Width / 2 + duct.Distance_from_floor + duct.Diameter / 2);
                y = duct.IsPositeve_x ? manhole.Height / 2 - duct.Distance_from_right - duct.Diameter / 2 : -(manhole.Height / 2 - duct.Distance_from_right - duct.Diameter / 2);
            }

            Circle ductCircle = new Circle
            {
                Center = new Point3d(origin.X + x, origin.Y + y, 0),
                Radius = duct.Diameter / 2
            };

            modelSpace.AppendEntity(ductCircle);
            trans.AddNewlyCreatedDBObject(ductCircle, true);
        }

        private void CreateNewLayoutTab(Document doc,int indx, string layoutName, Transaction trans, Point3d viewportCenter, double verticalHeight, double horizontalWidth)
        {

            // Create or get the layout
            LayoutManager layoutManager = LayoutManager.Current;
            ObjectId layoutId;

            if (!layoutManager.LayoutExists(layoutName))
            {
                layoutId = layoutManager.CreateLayout(layoutName);
            }
            else
            {
                layoutId = layoutManager.CreateLayout(layoutName + " manhole #" + indx + 1);
            }

            Layout layout = (Layout)trans.GetObject(layoutId, OpenMode.ForWrite);
            BlockTableRecord layoutBtr = (BlockTableRecord)trans.GetObject(layout.BlockTableRecordId, OpenMode.ForWrite);

            // Remove existing viewports to avoid duplicates
            foreach (ObjectId objId in layoutBtr)
            {
                Entity entity = (Entity)trans.GetObject(objId, OpenMode.ForWrite);
                if (entity is Viewport)
                {
                    entity.Erase();
                }
            }

            // Create and configure the new viewport
            Viewport viewport = new Viewport
            {
                CenterPoint = new Point3d(horizontalWidth / 2.0, verticalHeight / 2.0, 0), // Position within the layout
                ViewCenter = new Point2d(viewportCenter.X, viewportCenter.Y), // Target center of the plot
                ViewHeight = verticalHeight + 1, // Add a margin around the view
                Width = horizontalWidth + 1, // Add a margin around the view
                Height = verticalHeight + 1,
                ViewDirection = new Vector3d(0, 0, 1) // Standard top-down view
            };

            layoutBtr.AppendEntity(viewport);
            trans.AddNewlyCreatedDBObject(viewport, true);
            // Zoom to the extents of the viewport
           
            doc.SendStringToExecute("._zoom _extents ", true, false, false);
        }

        private void AddImage(Transaction trans, Database db, string imagePath, Point3d insertionPoint)
        {
            // Define the name for the image
            string imageName = "ReferenceImage";

            // Ensure the image dictionary exists
            ObjectId imageDictId = RasterImageDef.GetImageDictionary(db);
            if (imageDictId.IsNull)
            {
                imageDictId = RasterImageDef.CreateImageDictionary(db);
            }

            // Open the image dictionary
            DBDictionary imageDict = trans.GetObject(imageDictId, OpenMode.ForRead) as DBDictionary;

            // Check if the image definition already exists
            ObjectId imageDefId;
            RasterImageDef imageDef;
            if (imageDict.Contains(imageName))
            {
                imageDefId = imageDict.GetAt(imageName);
                imageDef = trans.GetObject(imageDefId, OpenMode.ForWrite) as RasterImageDef;
            }
            else
            {
                // Create a new image definition
                imageDef = new RasterImageDef
                {
                    SourceFileName = imagePath
                };
                imageDef.Load();

                // Add the image definition to the dictionary
                imageDict.UpgradeOpen();
                imageDefId = imageDict.SetAt(imageName, imageDef);
                trans.AddNewlyCreatedDBObject(imageDef, true);
            }

            // Open the current space (ModelSpace)
            BlockTable blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            // Create a new raster image
            RasterImage rasterImage = new RasterImage
            {
                ImageDefId = imageDefId
            };

            // Define a default scale for the image
            double scaleFactor_x= 1;
            double scaleFactor_y= 1;

            // Define the coordinate system for the image's orientation
            Vector3d width = new Vector3d(scaleFactor_x, 0, 0); 
            Vector3d height = new Vector3d(0, scaleFactor_y, 0); 
            CoordinateSystem3d coordinateSystem = new CoordinateSystem3d(insertionPoint, width, height);
            rasterImage.Orientation = coordinateSystem;

            // Add the raster image to ModelSpace
            modelSpace.AppendEntity(rasterImage);
            trans.AddNewlyCreatedDBObject(rasterImage, true);

            // Associate the raster image with its definition
            RasterImage.EnableReactors(true);
            rasterImage.AssociateRasterDef(imageDef);
        }


    }

}
