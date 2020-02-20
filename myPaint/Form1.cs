using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace myPaint
{
    enum mouseEventType
    {
        leftClickOnBlankSpace, leftClickOnNotSelectedObject, leftClickOnSelectedObject,
        leftClickInControlBox,
        leftClickOnRotateControlButton, leftClickOnZoomControlButton, leftClickOnExtraControlButton,
        overOnBlankSpace, overOnNotSelectedObject, overOnSelectedObject,
        overOnRotateControlButton, overOnZoomControlButton, overOnExtraControlButton, overInControlBox
    };

    //enum action { drawNewShape, fill}
    public partial class Form1 : Form
    {
        layer mainLayer;
        List<layer> listLayer = new List<layer>();
        Graphics temporaryGraphic, mainGraphic, displayGraphic;
        Bitmap temporaryBMP, mainBMP, displayBMP;
        bool isHoldLeftMouse = false, isDrawPoints = false;
        Point mouseDownPositionLeftClick = new Point(), previousPosition = new Point();
        mouseEventType mouseDown;


        bool isSaveFile = false;
        TextBox textEdit = new TextBox();
        int currentLayer = 0;

        myObject copiedObject=new myObject();
        bool isCopied=false;
        Random randomGenerator = new Random();

        bool isEditing = false;
        DialogResult saveQuestion;
        int colorButton = 0;

        Color layerBackground = Color.White, foreColor = Color.Black, fillColor = Color.Transparent;
        myObjectType type = myObjectType.RECTANGLE;
        outlineStyle currentOutline = outlineStyle.SOLID;
        int fillType = 0;
        textFormat textFormat = textFormat.CENTER_HORIZONTAL_ALIGN;
        FontStyle fontStyle = FontStyle.Regular;
        string font = "Calibri";
        float fontSize = 32;
        ContextMenu rightClickMenu=new ContextMenu();


        int thicknessBrush = 3;
        myObject myObject;
        List<PointF> lsPoint = new List<PointF>();
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            temporaryBMP = new Bitmap(drawingFrame.Width, drawingFrame.Height);
            mainBMP = new Bitmap(drawingFrame.Width, drawingFrame.Height);
            displayBMP = new Bitmap(drawingFrame.Width, drawingFrame.Height);
            temporaryGraphic = Graphics.FromImage(temporaryBMP);
            mainGraphic = Graphics.FromImage(mainBMP);
            displayGraphic = Graphics.FromImage(displayBMP);
            displayGraphic.Clear(layerBackground);
            displayGraphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            mainGraphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            mainLayer = new layer(layerBackground);
            //ContextMenu rightClickMenu = new ContextMenu();
            rightClickMenu.MenuItems.Add("Send to back", new EventHandler(rightClickMenu_SendTobackClick));
            rightClickMenu.MenuItems.Add("Bring to front", new EventHandler(rightClickMenu_BringToFrontClick));
            rightClickMenu.MenuItems.Add("Send backward", new EventHandler(rightClickMenu_SendBackwardClick));
            rightClickMenu.MenuItems.Add("Bring forward", new EventHandler(rightClickMenu_BringForwardClick));

            drawingFrame.Image = displayBMP;
            textEdit.Multiline = true;
            textEdit.LostFocus += new EventHandler(textEdit_mouseLeave);
            myObject.dpiY = mainGraphic.DpiY;
            color0.Click += new EventHandler(colorButton_Click);
            color2.Click += new EventHandler(colorButton_Click);
            color3.Click += new EventHandler(colorButton_Click);
            color4.Click += new EventHandler(colorButton_Click);
            color5.Click += new EventHandler(colorButton_Click);
            color6.Click += new EventHandler(colorButton_Click);
            color7.Click += new EventHandler(colorButton_Click);
            color8.Click += new EventHandler(colorButton_Click);
            color9.Click += new EventHandler(colorButton_Click);
            color10.Click += new EventHandler(colorButton_Click);
            color11.Click += new EventHandler(colorButton_Click);
            color12.Click += new EventHandler(colorButton_Click);
            color13.Click += new EventHandler(colorButton_Click);
            color14.Click += new EventHandler(colorButton_Click);
            color15.Click += new EventHandler(colorButton_Click);
            color16.Click += new EventHandler(colorButton_Click);
            color17.Click += new EventHandler(colorButton_Click);
            color18.Click += new EventHandler(colorButton_Click);
            color19.Click += new EventHandler(colorButton_Click);
            color20.Click += new EventHandler(colorButton_Click);
            color21.Click += new EventHandler(colorButton_Click);
            color22.Click += new EventHandler(colorButton_Click);
            color23.Click += new EventHandler(colorButton_Click);
            color24.Click += new EventHandler(colorButton_Click);
            color25.Click += new EventHandler(colorButton_Click);
            color26.Click += new EventHandler(colorButton_Click);
            color27.Click += new EventHandler(colorButton_Click);
            color28.Click += new EventHandler(colorButton_Click);
            color29.Click += new EventHandler(colorButton_Click);
            color30.Click += new EventHandler(colorButton_Click);
            colorDialog1.CustomColors = new int[] { 1, 2, 0 };
            for (int i = 1; i < FontFamily.Families.Length; i++)
                fontCombobox.Items.Add(FontFamily.Families[i].Name);
            fontCombobox.Text = "Calibri";
            fontCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
            //fontCombobox.SelectedIndex = 0;
            for (int i = 8; i < 72; i++)
                fontSizeCombobox.Items.Add(i);
            fontSizeCombobox.SelectedIndex = 10;
            fontSizeCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 1; i < 10; i++)
                brushThicknessCombobox.Items.Add(i);
            brushThicknessCombobox.SelectedIndex = 1;
            brushThicknessCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (outlineStyle temp in Enum.GetValues(typeof(outlineStyle)))
                outlineCombobox.Items.Add(temp.ToString());
            outlineCombobox.SelectedIndex = 0;
            outlineCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
            fillStyleCombobox.Items.Add("Solid");
            foreach (HatchStyle temp in Enum.GetValues(typeof(HatchStyle)))
                fillStyleCombobox.Items.Add(temp.ToString());
            fillStyleCombobox.SelectedIndex = 0;
            fillStyleCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
            listLayer.Add(mainLayer.clone());
        }

        private void updateSelectedObject()
        {
            if (mainLayer.isSelected != -1)
            {
                myObject = mainLayer.obArr[mainLayer.isSelected];
                if (myObject.type == myObjectType.TEXT)
                {
                    myObject.ChangeShapeProperties(currentOutline, fillType, thicknessBrush, foreColor, fillColor);
                    myObject.ChangeTextProperties(fillType, foreColor, fillColor, fontStyle, textFormat, fontSize, font);
                }
                else
                    myObject.ChangeShapeProperties(currentOutline, fillType, thicknessBrush, foreColor, fillColor);
                isSaveFile = false;
                updateListLayer();
            }
        }

        private void updateDrawFrame()
        {
            numberObjectLabel.Text = Convert.ToString(mainLayer.obArr.Count);
            mainLayer.draw(ref displayGraphic);
            drawingFrame.Refresh();
        }

        private void rightClickMenu_SendTobackClick(object sender, EventArgs e)
        {
            if (mainLayer.isSelected != -1)
            {
                if (mainLayer.isSelected != 0)
                {
                    myObject = mainLayer.obArr[mainLayer.isSelected];
                    mainLayer.obArr.RemoveAt(mainLayer.isSelected);
                    mainLayer.obArr.Insert(0, myObject);
                    mainLayer.isSelected = 0;
                    updateListLayer();
                    updateDrawFrame();
                }
            }
        }

        private void rightClickMenu_BringToFrontClick(object sender, EventArgs e)
        {
            if (mainLayer.isSelected != -1)
            {
                if (mainLayer.isSelected != mainLayer.obArr.Count - 1)
                {
                    myObject = mainLayer.obArr[mainLayer.isSelected];
                    mainLayer.obArr.RemoveAt(mainLayer.isSelected);
                    mainLayer.obArr.Add(myObject);
                    mainLayer.isSelected = mainLayer.obArr.Count - 1;
                    updateListLayer();
                    updateDrawFrame();
                }
            }
        }
        private void rightClickMenu_SendBackwardClick(object sender, EventArgs e)
        {
            if (mainLayer.isSelected != -1)
            {
                
                if (mainLayer.isSelected > 0)
                {
                    myObject = mainLayer.obArr[mainLayer.isSelected];
                    mainLayer.obArr[mainLayer.isSelected] = mainLayer.obArr[mainLayer.isSelected - 1];
                    mainLayer.obArr[mainLayer.isSelected - 1] = myObject;
                    mainLayer.isSelected--;
                    updateListLayer();
                    updateDrawFrame();
                }
                
                
            }
        }
        private void rightClickMenu_BringForwardClick(object sender, EventArgs e)
        {
            if (mainLayer.isSelected != -1)
            {

                if (mainLayer.isSelected < mainLayer.obArr.Count-1)
                {
                    myObject = mainLayer.obArr[mainLayer.isSelected];
                    mainLayer.obArr[mainLayer.isSelected] = mainLayer.obArr[mainLayer.isSelected + 1];
                    mainLayer.obArr[mainLayer.isSelected + 1] = myObject;
                    mainLayer.isSelected++;
                    updateListLayer();
                    updateDrawFrame();
                }


            }
        }

        private void colorButton_Click(object sender, EventArgs e)
        {
            switch (colorButton)
            {
                case 0:
                    foreColor = ((Button)sender).BackColor;
                    updateSelectedObject();
                    updateDrawFrame();
                    foregroundColorButton.BackColor = ((Button)sender).BackColor;
                    break;
                case 1:
                    fillColor = ((Button)sender).BackColor;
                    updateSelectedObject();
                    updateDrawFrame();
                    fillColorButton.BackColor = ((Button)sender).BackColor;
                    break;
                case 2:
                    layerBackground = ((Button)sender).BackColor;
                    mainLayer.background = ((Button)sender).BackColor;
                    mainLayer.draw(ref displayGraphic);
                    drawingFrame.Refresh();
                    updateListLayer();
                    backgroundColorButton.BackColor = ((Button)sender).BackColor;
                    break;
            }
        }

        private void updateListLayer()
        {
            if (listLayer.Count >= 100)
            {
                listLayer.RemoveAt(0);
                currentLayer--;
            }
            for (int i = listLayer.Count-1; i>currentLayer; i--)
                listLayer.RemoveAt(i);

            listLayer.Add(mainLayer.clone());
            currentLayer++;
            numberLayerLabel.Text = Convert.ToString(listLayer.Count);
        }

        private void textEdit_mouseLeave(object sender, EventArgs e)
        {

            if (myObject.getText() != ((TextBox)sender).Text)
            {
                myObject.ChangeText(((TextBox)textEdit).Text);
                drawingFrame.Controls.Remove(textEdit);
                mainLayer.draw(ref displayGraphic);
                drawingFrame.Refresh();
                isEditing = false;

                isSaveFile = false;
                updateListLayer();
            }
        }

        private void drawingFrame_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isHoldLeftMouse = true;
                mouseDownPositionLeftClick = e.Location;
                mouseDown = mainLayer.onMouseDown(e.Location);
                mainGraphic.Clear(layerBackground);
                if (isDrawPoints)
                    mouseDown = mouseEventType.leftClickOnBlankSpace;
                if (isEditing)
                {
                    if (e.Location.X < textEdit.Location.X || e.Location.X > (textEdit.Location.X + textEdit.Width) || e.Location.Y < textEdit.Location.Y || e.Location.Y > (textEdit.Location.Y + textEdit.Height))
                    {
                        if (myObject.getText() != textEdit.Text)
                        {
                            myObject.ChangeText(textEdit.Text);
                            drawingFrame.Controls.Remove(textEdit);
                            mainLayer.draw(ref displayGraphic);
                            drawingFrame.Refresh();
                            isEditing = false;

                            isSaveFile = false;
                           updateListLayer();
                        }
                    }
                }
                switch (mouseDown)
                {
                    case mouseEventType.leftClickOnBlankSpace:
                        switch (type)
                        {
                            case myObjectType.LINE:
                                myObject = new myLine(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.RECTANGLE:
                                myObject = new myRectangle(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.PARALLELOGRAM:
                                myObject = new myParallelogram(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.POLYGON:
                                if (isDrawPoints == false)
                                {
                                    isDrawPoints = true;
                                    myObject = new myPolygon(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                    lsPoint.Clear();
                                }
                                if (lsPoint.Count > 2 && Math.Abs(e.Location.X - lsPoint[0].X) < 10 && Math.Abs(e.Location.Y - lsPoint[0].Y) < 10)
                                {
                                    //MessageBox.Show("test");
                                    lsPoint.Add(lsPoint[0]);
                                    myObject.adjust(lsPoint);
                                    lsPoint.Clear();
                                    mainLayer.addShape(myObject);
                                    isDrawPoints = false;
                                    isSaveFile = false;
                                    return;
                                }
                                else
                                    lsPoint.Add(e.Location);

                                break;
                            case myObjectType.BROKENLINE:
                                if (isDrawPoints == false)
                                {
                                    isDrawPoints = true;
                                    myObject = new myBrokenLine(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                    lsPoint.Clear();
                                }
                                lsPoint.Add(e.Location);
                                //myObject = new myBrokenLine(currentOutline, thicknessBrush, foreColor, Color.Transparent, fillColor);
                                break;
                            case myObjectType.CIRCLE:
                                myObject = new myCircle(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.CIRCLEARC:
                                myObject = new myCircleArc(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.ELLIPSE:
                                myObject = new myEllipse(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.ELLIPSEARC:
                                myObject = new myEllipseArc(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.BEZIERCURVE:
                                if (isDrawPoints == false)
                                {
                                    isDrawPoints = true;
                                    myObject = new myBezierCurve(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                    lsPoint.Clear();
                                }
                                lsPoint.Add(e.Location);
                                if (lsPoint.Count == 4)
                                {
                                    myObject.adjust(lsPoint);
                                    lsPoint.Clear();
                                    mainLayer.addShape(myObject);
                                    isDrawPoints = false;
                                    isSaveFile = false;
                                    return;
                                }
                                //myObject = new myRectangle(currentOutline, thicknessBrush, foreColor, Color.Transparent, fillColor);
                                break;
                            case myObjectType.TEXT:
                                myObject = new myText(currentOutline, thicknessBrush, foreColor, fillColor, fillType, font, fontSize, textFormat, fontStyle);

                                break;
                            case myObjectType.PARABOLA:
                                myObject = new myParabola(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;
                            case myObjectType.HYPEBOLA:
                                myObject = new myHypebola(currentOutline, thicknessBrush, foreColor, fillColor, fillType);
                                break;

                        }
                        mainLayer.draw(ref mainGraphic);
                        break;
                    case mouseEventType.leftClickInControlBox:
                    case mouseEventType.leftClickOnSelectedObject:
                    case mouseEventType.leftClickOnNotSelectedObject:
                    case mouseEventType.leftClickOnZoomControlButton:
                    case mouseEventType.leftClickOnExtraControlButton:
                    case mouseEventType.leftClickOnRotateControlButton:
                        myObject = mainLayer.getSelectedObject();
                        //mainLayer.isSelected=
                        mainLayer.drawWithoutSelected(ref mainGraphic);
                        break;
                }

            }
            else
            {
                mouseDown = mainLayer.onMouseDown(e.Location);
                switch (mouseDown) {
                    case mouseEventType.leftClickInControlBox:
                    case mouseEventType.leftClickOnSelectedObject:
                    case mouseEventType.leftClickOnNotSelectedObject:
                    case mouseEventType.leftClickOnZoomControlButton:
                    case mouseEventType.leftClickOnExtraControlButton:
                    case mouseEventType.leftClickOnRotateControlButton:
                        rightClickMenu.Show(drawingFrame, e.Location);
                        break;
                }
            }
        }

        private void drawingFrame_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawPoints)
            {
                temporaryGraphic.Clear(Color.Transparent);
                lsPoint.Add(e.Location);
                myObject.adjust(lsPoint,true);
                lsPoint.RemoveAt(lsPoint.Count - 1);
                myObject.draw(ref temporaryGraphic);
                displayGraphic.Clear(layerBackground);
                displayGraphic.DrawImage(mainBMP, new Point(0, 0));
                displayGraphic.DrawImage(temporaryBMP, new Point(0, 0));
                drawingFrame.Refresh();
            }
            if (isHoldLeftMouse)
            {
                temporaryGraphic.Clear(Color.Transparent);
                switch (mouseDown)
                {
                    case mouseEventType.leftClickOnBlankSpace:
                        myObject.adjust(mouseDownPositionLeftClick, e.Location, true);
                        myObject.draw(ref temporaryGraphic); 
                        break;
                    case mouseEventType.leftClickInControlBox:
                    case mouseEventType.leftClickOnSelectedObject:
                    case mouseEventType.leftClickOnNotSelectedObject:
                        myObject.translate(previousPosition, e.Location);
                        myObject.draw(ref temporaryGraphic);
                        myObject.drawControlBox(ref temporaryGraphic);
                        break;
                    case mouseEventType.leftClickOnZoomControlButton:
                        myObject.scale(previousPosition, e.Location);
                        myObject.draw(ref temporaryGraphic);
                        myObject.drawControlBox(ref temporaryGraphic);
                        break;
                    case mouseEventType.leftClickOnRotateControlButton:
                        myObject.rotate(previousPosition, e.Location);
                        myObject.draw(ref temporaryGraphic);
                        myObject.drawControlBox(ref temporaryGraphic);
                        break;
                    case mouseEventType.leftClickOnExtraControlButton:
                        myObject.moveExtraPoint(previousPosition, e.Location);
                        myObject.draw(ref temporaryGraphic);
                        myObject.drawControlBox(ref temporaryGraphic);

                        break;
                }


                displayGraphic.Clear(layerBackground);
                displayGraphic.DrawImage(mainBMP, new Point(0, 0));
                displayGraphic.DrawImage(temporaryBMP, new Point(0, 0));
                drawingFrame.Refresh();
            }
            else
            {
                switch (mainLayer.onMouseMove(e.Location))
                {
                    case mouseEventType.overOnBlankSpace:
                        drawingFrame.Cursor = Cursors.Cross;
                        break;
                    case mouseEventType.overOnNotSelectedObject:
                        drawingFrame.Cursor = Cursors.Hand;
                        break;
                    case mouseEventType.overInControlBox:
                    case mouseEventType.overOnSelectedObject:
                        drawingFrame.Cursor = Cursors.SizeAll;
                        break;
                    case mouseEventType.overOnZoomControlButton:
                        drawingFrame.Cursor = Cursors.SizeNESW;
                        break;
                    case mouseEventType.overOnRotateControlButton:
                    case mouseEventType.overOnExtraControlButton:
                        drawingFrame.Cursor = Cursors.Hand;
                        break;
                }
            }
            previousPosition = e.Location;
            //label2.Text = Convert.ToString(isDrawPoints);
            //label3.Text = Convert.ToString(mainLayer.obArr.Count);
        }

        private void drawingFrame_MouseUp(object sender, MouseEventArgs e)
        {
            isHoldLeftMouse = false;
            if (isDrawPoints)
                return;
            
            if (mouseDownPositionLeftClick == e.Location)
            {
                updateDrawFrame();
                return;
            }

            switch (mouseDown)
            {
                case mouseEventType.leftClickOnBlankSpace:
                    if (Math.Abs(mouseDownPositionLeftClick.X - e.Location.X) > 3 && Math.Abs(mouseDownPositionLeftClick.Y - e.Location.Y) > 3)
                    {
                        myObject.adjust(mouseDownPositionLeftClick, e.Location, false);
                        mainLayer.addShape(myObject);
                        //listLayer.Add(mainLayer);

                        isSaveFile = false;
                        updateListLayer();

                    }
                    break;
                case mouseEventType.leftClickInControlBox:
                case mouseEventType.leftClickOnSelectedObject:
                case mouseEventType.leftClickOnNotSelectedObject:
                    myObject.translate(previousPosition, e.Location);
                    if (mouseDownPositionLeftClick != e.Location)
                    {
                        isSaveFile = false;
                        updateListLayer();
                    }
                    //obArr[isSelected]=sh;
                    break;
                case mouseEventType.leftClickOnRotateControlButton:
                    myObject.rotate(previousPosition, e.Location);
                    if (mouseDownPositionLeftClick != e.Location)
                    {
                        isSaveFile = false;
                        updateListLayer();
                    }
                    //listLayer.Add(mainLayer);
                    //obArr[isSelected]=sh;
                    break; ;
                case mouseEventType.leftClickOnZoomControlButton:
                    myObject.scale(previousPosition, e.Location);
                    if (mouseDownPositionLeftClick != e.Location)
                    {
                        isSaveFile = false;
                        updateListLayer();
                    }//obArr[isSelected]=sh;
                    break;
                case mouseEventType.leftClickOnExtraControlButton:
                    myObject.moveExtraPoint(previousPosition, e.Location);
                    //isSaveFile = false;
                    if (mouseDownPositionLeftClick != e.Location)
                    {
                        isSaveFile = false;
                        updateListLayer();
                    }
                    //obArr[isSelected] = sh;
                    break;
            }
            //displayGraphic.Clear(layerBackground);
            mainLayer.draw(ref displayGraphic);
            drawingFrame.Refresh();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isSaveFile == false)
            {
                saveQuestion = MessageBox.Show("(All changes will be lost!)\nAre you sure to close without saving? ", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (saveQuestion == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void authorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("  Author:\n+ Vũ Thanh Tùng - 1612791\n+ Nguyễn Thanh Tuấn - 1612774\n(16CNTN_HCMUS)", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void regularTextButton_Click(object sender, EventArgs e)
        {
            fontStyle = FontStyle.Regular;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void boldTextButton_Click(object sender, EventArgs e)
        {
            fontStyle = FontStyle.Bold;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void ItalicTextButton_Click(object sender, EventArgs e)
        {
            fontStyle = FontStyle.Italic;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void underlineTextButton_Click(object sender, EventArgs e)
        {
            fontStyle = FontStyle.Underline;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void strikeTextButton_Click(object sender, EventArgs e)
        {
            fontStyle = FontStyle.Strikeout;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void leftAlignButton_Click(object sender, EventArgs e)
        {
            textFormat = textFormat.LEFT_HORIZONTAL_ALIGN;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void rightAllignButton_Click(object sender, EventArgs e)
        {
            textFormat = textFormat.RIGHT_HORIZONTAL_ALIGN;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void centerAllignButton_Click(object sender, EventArgs e)
        {
            textFormat = textFormat.CENTER_HORIZONTAL_ALIGN;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            mainLayer = new layer(layerBackground);
            temporaryGraphic.Clear(layerBackground);
            mainGraphic.Clear(layerBackground);
            displayGraphic.Clear(layerBackground);
            isSaveFile = false;
            drawingFrame.Refresh();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveFileDialog.Title = "Save file";
            saveFileDialog.Filter = "(*.dat)|*.dat";
            saveFileDialog.FilterIndex = 0;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                BinaryWriter file = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate));
                mainLayer.save(file);
                file.Close();
                isSaveFile = true;
            }
        }

        private void foregroundColorButton_Click(object sender, EventArgs e)
        {
            colorButton = 0;
        }

        private void fillColorButton_Click(object sender, EventArgs e)
        {
            colorButton = 1;
        }

        private void backgroundColorButton_Click(object sender, EventArgs e)
        {
            colorButton = 2;
        }

        private void editColorButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                switch (colorButton)
                {
                    case 0:
                        foreColor = colorDialog1.Color;
                        updateSelectedObject();
                        updateDrawFrame();
                        foregroundColorButton.BackColor = colorDialog1.Color;
                        break;
                    case 1:
                        fillColor = colorDialog1.Color;
                        updateSelectedObject();
                        updateDrawFrame();
                        fillColorButton.BackColor = colorDialog1.Color;
                        break;
                    case 2:
                        layerBackground = colorDialog1.Color;
                        mainLayer.background = colorDialog1.Color;
                        mainLayer.draw(ref displayGraphic);
                        drawingFrame.Refresh();
                        updateListLayer();
                        backgroundColorButton.BackColor = colorDialog1.Color;
                        break;
                }

            }

        }

        private void outlineCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentOutline = (outlineStyle)outlineCombobox.SelectedIndex;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void brushThicknessCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            thicknessBrush = Convert.ToInt32(brushThicknessCombobox.Text);
            updateSelectedObject();
            updateDrawFrame();
        }

        private void fillStyleCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillType = fillStyleCombobox.SelectedIndex;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void fontCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            font = fontCombobox.Text;
            updateSelectedObject();
            updateDrawFrame();
        }

        private void fontSizeCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            fontSize = Convert.ToSingle(fontSizeCombobox.Text);
            updateSelectedObject();
            updateDrawFrame();
        }

        private void lineButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.LINE;
        }

        private void rectangleButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.RECTANGLE;
        }

        private void parallelogramButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.PARALLELOGRAM;
        }

        private void polygonButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.POLYGON;
        }

        private void polylinesButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.BROKENLINE;
        }

        private void circleButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.CIRCLE;
        }

        private void circleArcButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.CIRCLEARC;
        }

        private void ellipseButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.ELLIPSE;
        }

        private void ellipseArcButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.ELLIPSEARC;
        }

        private void bezierCurveButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.BEZIERCURVE;
        }

        private void parabolButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.PARABOLA;
        }

        private void HypebolButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.HYPEBOLA;
        }

        private void drawTextButton_Click(object sender, EventArgs e)
        {
            type = myObjectType.TEXT;
        }

        private void undoButton_Click(object sender, EventArgs e)
        {
            if (currentLayer > 0) {
                currentLayer--;
                mainLayer = listLayer[currentLayer];
                updateDrawFrame();
            }
                
        }

        private void redoButton_Click(object sender, EventArgs e)
        {
            if (currentLayer != listLayer.Count - 1)
            {
                currentLayer++;
                mainLayer = listLayer[currentLayer];
                updateDrawFrame();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isSaveFile == false)
            {
                saveQuestion = MessageBox.Show("(All changes will be lost!)\nAre you sure to close without saving? ", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (saveQuestion == DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                mainLayer = new layer(layerBackground);
                listLayer.Clear();
                listLayer.Add(mainLayer.clone());
                currentLayer = 0;
                temporaryGraphic.Clear(layerBackground);
                mainGraphic.Clear(layerBackground);
                displayGraphic.Clear(layerBackground);
                isSaveFile = false;
                drawingFrame.Refresh();
            }
            
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Title = "Open file";
            openFileDialog.Filter = "(*.dat)|*.dat";
            openFileDialog.FilterIndex = 0;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                BinaryReader file = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.OpenOrCreate));
                mainLayer.load(file);
                listLayer.Clear();
                listLayer.Add(mainLayer.clone());
                currentLayer = 0;
                file.Close();
                mainLayer.draw(ref displayGraphic);
                drawingFrame.Refresh();
                isSaveFile = true;
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.Title = "Save to Image";
            saveFileDialog.Filter = "(*.bmp)|*.bmp|(*.jpeg)|*.jpeg|(*.png)|*.png|(*.gif)|*.gif|(*.tif)|*.tif";
            saveFileDialog.FilterIndex = 0;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                mainLayer.clearSelected();
                mainLayer.draw(ref displayGraphic);
                displayBMP.Save(saveFileDialog.FileName);
            }
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.Title = "Save file";
            saveFileDialog.Filter = "(*.dat)|*.dat";
            saveFileDialog.FilterIndex = 0;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                BinaryWriter file = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate));
                mainLayer.save(file);
                file.Close();
                isSaveFile = true;
            }

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                mainLayer.deleteSelectedObject();
                mainLayer.draw(ref displayGraphic);
                updateDrawFrame();
                updateListLayer();

            }
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                if (mainLayer.isSelected != -1)
                {
                    copiedObject = mainLayer.obArr[mainLayer.isSelected].clone();
                    isCopied = true;
                }
            }
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                if (isCopied)
                {
                    //copiedObject.translate(new PointF(drawingFrame.Width / 2, drawingFrame.Height / 2), new PointF(drawingFrame.Width / 2 + randomGenerator.Next(-10, 10), drawingFrame.Height + randomGenerator.Next(-10, 10)));
                    mainLayer.addShape(copiedObject);
                    copiedObject = copiedObject.clone();
                    //copiedObject = mainLayer.obArr[mainLayer.obArr.Count-1].clone();
                    updateListLayer();
                    updateDrawFrame();
                }
            }
        }

        private void drawingFrame_DoubleClick(object sender, EventArgs e)
        {
            if (isDrawPoints)
            {
                switch (type)
                {
                    case myObjectType.POLYGON:
                        lsPoint.Add(lsPoint[0]);
                        myObject.adjust(lsPoint);
                        lsPoint.Clear();
                        mainLayer.addShape(myObject);
                        isDrawPoints = false;
                        isSaveFile = false;
                        updateListLayer();
                        break;
                    case myObjectType.BROKENLINE:
                        myObject.adjust(lsPoint);
                        lsPoint.Clear();
                        mainLayer.addShape(myObject);
                        isDrawPoints = false;
                        isSaveFile = false;
                        updateListLayer();
                        break;
                        
                }

            }
            else
            {
                myObject = mainLayer.getSelectedObject();
                if (myObject.type == myObjectType.TEXT)
                {
                    myObject.setTextBox(ref textEdit);
                    drawingFrame.Controls.Add(textEdit);
                    textEdit.Show();
                    isEditing = true;
                }
            }
        }
    }
}
