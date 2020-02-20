using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

enum outlineStyle { SOLID, DOT, DASH, DASHDOT, DASHDOTDOT, NULL };
enum myObjectType { LINE, RECTANGLE, PARALLELOGRAM, POLYGON, BROKENLINE, CIRCLE, ELLIPSE, CIRCLEARC, ELLIPSEARC, BEZIERCURVE, TEXT, PARABOLA, HYPEBOLA };
//enum DrawMouseEventType={
enum textFormat { LEFT_HORIZONTAL_ALIGN, RIGHT_HORIZONTAL_ALIGN, CENTER_HORIZONTAL_ALIGN, TOP_VERTICAL_ALIGN, BOTTOM_VERTIAL_ALIGN, CENTER_VERTICAL_ALIGN };

namespace myPaint
{
    public static class Transform
    {
        public static float temp;
        public static Matrix result_mt = new Matrix(), rotate_mt = new Matrix(), translate_mt = new Matrix(), scale_mt = new Matrix(), rerotate_mt = new Matrix(), retranslate_mt = new Matrix();
        public static List<PointF> lsPoint = new List<PointF>();
        public static Matrix temporary = new Matrix(), temporary1 = new Matrix(), temporary2 = new Matrix();
        public static PointF[] arrPoint;

    }


    class myObject
    {
        public static float dpiY;
        protected outlineStyle outline;
        protected int fillType;
        protected int brushThickness;
        protected Color foregroundColor, fillColor;
        protected GraphicsPath gp;
        protected Pen pen;
        protected Brush brush;
        public double angle = 0;
        public myObjectType type;
        protected controlBox ctrl;

        public myObject() { }

        public myObject(outlineStyle br, int thick, Color fore, Color fill, int filltype)
        {
            outline = br;
            brushThickness = thick;
            foregroundColor = fore;
            fillColor = fill;
            gp = new GraphicsPath();
            fillType = filltype;
            createPen();
            createBrush();
            ctrl = new controlBox();
        }

        public virtual myObject clone()
        {
            return new myObject();
        }

        protected void createPen()
        {
            switch (outline)
            {
                case outlineStyle.SOLID:
                    pen = new Pen(foregroundColor, brushThickness);
                    pen.DashStyle = DashStyle.Solid;
                    break;
                case outlineStyle.DASH:
                    pen = new Pen(foregroundColor, brushThickness);
                    pen.DashStyle = DashStyle.Dash;
                    break;
                case outlineStyle.DASHDOT:
                    pen = new Pen(foregroundColor, brushThickness);
                    pen.DashStyle = DashStyle.DashDot;
                    break;
                case outlineStyle.DASHDOTDOT:
                    pen = new Pen(foregroundColor, brushThickness);
                    pen.DashStyle = DashStyle.DashDotDot;
                    break;
                case outlineStyle.DOT:
                    pen = new Pen(foregroundColor, brushThickness);
                    pen.DashStyle = DashStyle.Dot;
                    break;
                case outlineStyle.NULL:
                    pen = new Pen(Color.Transparent, brushThickness);
                    pen.DashStyle = 0;
                    break;
            }
        }

        protected void createBrush()
        {
            if (fillType == 0)
                brush = new SolidBrush(fillColor);
            else
                brush = new HatchBrush((HatchStyle)(fillType - 1), foregroundColor, fillColor);
        }

        public virtual void save(BinaryWriter f)
        {
            f.Write(Convert.ToInt32(outline));
            f.Write(Convert.ToInt32(fillType));
            f.Write(Convert.ToInt32(brushThickness));
            f.Write(foregroundColor.A);
            f.Write(foregroundColor.R);
            f.Write(foregroundColor.G);
            f.Write(foregroundColor.B);
            f.Write(fillColor.A);
            f.Write(fillColor.R);
            f.Write(fillColor.G);
            f.Write(fillColor.B);
            f.Write(angle);
        }

        public virtual void load(BinaryReader f)
        {
            byte a, r, g, b;
            outline = (outlineStyle)f.ReadInt32();
            fillType = f.ReadInt32();
            brushThickness = f.ReadInt32();
            a = f.ReadByte();
            r = f.ReadByte();
            g = f.ReadByte();
            b = f.ReadByte();
            foregroundColor = Color.FromArgb(a, r, g, b);
            a = f.ReadByte();
            r = f.ReadByte();
            g = f.ReadByte();
            b = f.ReadByte();
            fillColor = Color.FromArgb(a, r, g, b);
            angle = f.ReadDouble();
            createBrush();
            createPen();
        }


        public virtual void draw(ref Graphics g)
        {
        }

        public virtual void drawControlBox(ref Graphics g) { }

        public virtual void adjust(PointF p0, PointF p1, bool isMoving = false) { }

        public virtual void adjust(List<PointF> p, bool isMoving = false) { }

        public mouseEventType onMouseDown(PointF p, bool isSelected)
        {
            mouseEventType ans = mouseEventType.leftClickOnBlankSpace;

            if (isSelected)
                ans = ctrl.onMouseDown(p);
            if (ans != mouseEventType.leftClickOnBlankSpace)
                return ans;
            if (gp.IsVisible(p))
                return mouseEventType.leftClickOnNotSelectedObject;
            PointF temp = p;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    temp.X += i;
                    temp.Y += j;
                    if (gp.IsOutlineVisible(temp, pen))
                        return mouseEventType.leftClickOnNotSelectedObject;
                }

            return ans;
        }

        public mouseEventType onMouseMove(PointF p, bool isSelected)
        {
            mouseEventType ans = mouseEventType.overOnBlankSpace;

            if (isSelected)
                ans = ctrl.onMouseMove(p);
            if (ans != mouseEventType.overOnBlankSpace)
                return ans;
            if (gp.IsVisible(p))
                return mouseEventType.overOnNotSelectedObject;
            PointF temp = p;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    temp.X += i;
                    temp.Y += j;
                    if (gp.IsOutlineVisible(temp, pen))
                        return mouseEventType.overOnNotSelectedObject;
                }

            return ans;
        }

        public virtual void translate(PointF startPoint, PointF endPoint)
        {
            Transform.translate_mt = new Matrix();
            Transform.translate_mt.Translate(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
            Transform.lsPoint.Clear();
            Transform.lsPoint.Add(ctrl.p0);
            Transform.lsPoint.Add(ctrl.p1);
            Transform.lsPoint.Add(ctrl.center);
            for (int i = 0; i < ctrl.cbArr.Count; i++)
                Transform.lsPoint.Add(ctrl.cbArr[i].p0);
            Transform.arrPoint = Transform.lsPoint.ToArray();
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            ctrl.p0 = Transform.arrPoint[0];
            ctrl.p1 = Transform.arrPoint[1];
            ctrl.center = Transform.arrPoint[2];
            for (int i = 0; i < ctrl.cbArr.Count; i++)
                ctrl.cbArr[i].p0 = Transform.arrPoint[i + 3];
            ctrl.gp.Transform(Transform.translate_mt);
            gp.Transform(Transform.translate_mt);
        }

        public virtual void rotate(PointF startPoint, PointF endPoint)
        {

            float xvec0 = startPoint.X - ctrl.center.X, yvec0 = startPoint.Y - ctrl.center.Y, xvec1 = endPoint.X - ctrl.center.X, yvec1 = endPoint.Y - ctrl.center.Y;
            double temp = (xvec0 * xvec1 + yvec0 * yvec1) / (Math.Sqrt(xvec0 * xvec0 + yvec0 * yvec0) * Math.Sqrt(xvec1 * xvec1 + yvec1 * yvec1));
            double a = Math.Acos(Math.Min(1, (temp)));
            a = a * (180 / Math.PI);
            if ((startPoint.Y - ctrl.center.Y) * (endPoint.X - ctrl.center.X) + (ctrl.center.X - startPoint.X) * (endPoint.Y - ctrl.center.Y) > 0)
                a = -a;
            Transform.rotate_mt = new Matrix();
            if (angle > 360)
                angle = angle - 360;
            else if (angle < -360)
                angle = angle + 360;
            //else
            angle += a;
            Transform.rotate_mt.RotateAt(Convert.ToSingle(a), ctrl.center);
            Transform.lsPoint.Clear();
            for (int i = 0; i < ctrl.cbArr.Count; i++)
                Transform.lsPoint.Add(ctrl.cbArr[i].p0);
            Transform.arrPoint = Transform.lsPoint.ToArray();
            Transform.rotate_mt.TransformPoints(Transform.arrPoint);
            for (int i = 0; i < ctrl.cbArr.Count; i++)
                ctrl.cbArr[i].p0 = Transform.arrPoint[i];
            ctrl.gp.Transform(Transform.rotate_mt);
            gp.Transform(Transform.rotate_mt);
        }

        public virtual bool scale(PointF startPoint, PointF endPoint)
        {
            Type_ControlButton type = Type_ControlButton.bottomLeftZoom;
            float xScale = 1, yScale = 1;
            PointF centerPoint = new PointF();
            for (int i = 0; i < ctrl.cbArr.Count; i++)
            {
                if (ctrl.cbArr[i].onMouseDown(startPoint) == mouseEventType.leftClickOnZoomControlButton)
                {
                    type = ctrl.cbArr[i].type;
                }
            }

            Transform.lsPoint.Clear();
            Transform.lsPoint.Add(endPoint);
            for (int i = 0; i < ctrl.cbArr.Count; i++)
                Transform.lsPoint.Add(ctrl.cbArr[i].p0);
            Transform.arrPoint = Transform.lsPoint.ToArray();
            //Transform.
            switch (type)
            {
                case Type_ControlButton.bottomLeftZoom:
                    centerPoint = Transform.arrPoint[3];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];
                    if (Math.Abs(endPoint.X - 0) < 5)
                        return false;
                    if (Math.Abs(endPoint.Y - 0) < 5)
                        return false;
                    xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[7].X);
                    yScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[7].Y);
                    break;
                case Type_ControlButton.bottomRightZoom:
                    centerPoint = Transform.arrPoint[1];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];
                    if (Math.Abs(endPoint.X - 0) < 5)
                        return false;
                    if (Math.Abs(endPoint.Y - 0) < 5)
                        return false;
                    xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[5].X);
                    yScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[5].Y);
                    break;
                case Type_ControlButton.topLeftZoom:
                    centerPoint = Transform.arrPoint[5];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];
                    if (Math.Abs(endPoint.X) < 5)
                        return false;
                    if (Math.Abs(endPoint.Y) < 5)
                        return false;
                    xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[1].X);
                    yScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[1].Y);
                    break;
                case Type_ControlButton.topRightZoom:
                    centerPoint = Transform.arrPoint[7];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];
                    if (Math.Abs(endPoint.X) < 5)
                        return false;
                    if (Math.Abs(endPoint.Y) < 5)
                        return false;
                    xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[3].X);
                    yScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[3].Y);
                    break;
                case Type_ControlButton.topZoom:
                    centerPoint = Transform.arrPoint[6];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];

                    if (Math.Abs(endPoint.Y - 0) < 5)
                        return false;
                    //xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[0].X);
                    yScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[2].Y);
                    break;
                case Type_ControlButton.leftZoom:
                    centerPoint = Transform.arrPoint[4];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];

                    if (Math.Abs(endPoint.X) < 5)
                        return false;
                    //xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[0].X);
                    xScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[8].Y);
                    break;
                case Type_ControlButton.rightZoom:
                    centerPoint = Transform.arrPoint[8];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];

                    if (Math.Abs(endPoint.X) < 5)
                        return false;
                    //xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[0].X);
                    xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[4].X);
                    break;
                case Type_ControlButton.bottomZoom:
                    centerPoint = Transform.arrPoint[2];
                    Transform.translate_mt = new Matrix();
                    Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);
                    Transform.rotate_mt = new Matrix();
                    Transform.rotate_mt.Rotate(Convert.ToSingle(-angle));
                    Transform.translate_mt.TransformPoints(Transform.arrPoint);
                    Transform.rotate_mt.TransformPoints(Transform.arrPoint);
                    endPoint = Transform.arrPoint[0];

                    if (Math.Abs(endPoint.Y - 0) < 5)
                        return false;
                    //xScale = Convert.ToSingle(endPoint.X) / (Transform.arrPoint[0].X);
                    yScale = Convert.ToSingle(endPoint.Y) / (Transform.arrPoint[6].Y);
                    break;
            }

            //Transform.translate_mt = new Matrix();
            //Transform.translate_mt.Translate(-centerPoint.X, -centerPoint.Y);

            Transform.scale_mt = new Matrix();
            Transform.scale_mt.Scale(xScale, yScale);
            Transform.rerotate_mt = new Matrix();
            Transform.rerotate_mt.Rotate(Convert.ToSingle(angle));
            Transform.retranslate_mt = new Matrix();
            Transform.retranslate_mt.Translate(centerPoint.X, centerPoint.Y);


            Transform.scale_mt.Multiply(Transform.rerotate_mt, MatrixOrder.Append);
            Transform.scale_mt.Multiply(Transform.retranslate_mt, MatrixOrder.Append);

            Transform.temporary = new Matrix();
            Transform.temporary.Translate(-centerPoint.X, -centerPoint.Y);
            Transform.temporary.Multiply(Transform.rotate_mt, MatrixOrder.Append);
            Transform.temporary.Multiply(Transform.scale_mt, MatrixOrder.Append);


            Transform.scale_mt.TransformPoints(Transform.arrPoint);
            //Transform.rerotate_mt.TransformPoints(Transform.arrPoint);
            //Transform.retranslate_mt.TransformPoints(Transform.arrPoint);
            //Transform.retr_mt.TransformPoints(Transform.arrPoint);


            for (int i = 0; i < ctrl.cbArr.Count; i++)
                ctrl.cbArr[i].p0 = Transform.arrPoint[i + 1];
            ctrl.gp.Transform(Transform.temporary);
            //Transform.translate_mt.Multiply(Transform.scale_mt, MatrixOrder.Append);
            Transform.temporary1 = new Matrix();
            Transform.temporary1.RotateAt(Convert.ToSingle(angle), ctrl.center);

            Transform.temporary1.Multiply(Transform.temporary, MatrixOrder.Append);
            Transform.arrPoint = new PointF[] { ctrl.p0, ctrl.p1, ctrl.center };
            Transform.temporary1.TransformPoints(Transform.arrPoint);
            Transform.temporary2 = new Matrix();
            Transform.temporary2.RotateAt(Convert.ToSingle(-angle), Transform.arrPoint[2]);
            Transform.temporary2.TransformPoints(Transform.arrPoint);
            ctrl.p0 = Transform.arrPoint[0];
            ctrl.p1 = Transform.arrPoint[1];
            ctrl.center = Transform.arrPoint[2];

            gp.Transform(Transform.temporary);
            return true;
        }

        public virtual void moveExtraPoint(PointF startPoint, PointF endPoint) { }

        public void ChangeShapeProperties(outlineStyle outlineStyle, int fillStyle, int thick, Color foreColor, Color fill)
        {
            if (foreColor != foregroundColor || outlineStyle != outline || thick != brushThickness)
            {
                outline = outlineStyle;
                brushThickness = thick;
                foregroundColor = foreColor;
                createPen();
            }
            if (fill != fillColor || fillStyle != fillType)
            {
                fillType = fillStyle;
                fillColor = fill;
                createBrush();
            }

        }

        public virtual void ChangeTextProperties(int fillStyle, Color foreColor, Color fill, FontStyle style, textFormat formatText, float size, string fontType)
        {

        }

        public virtual void ChangeText(string text)
        {

        }

        public virtual void setTextBox(ref TextBox t) { }

        public virtual string getText() { return ""; }
    }


    class myText : myObject
    {
        string contentString;
        PointF p0, p1;
        string fontName;
        float fontSize;
        textFormat format;
        FontStyle fontStyle;

        public myText() { }

        public myText(outlineStyle br, int thick, Color fore, Color fill, int filltype, string font, float size, textFormat formatType, FontStyle style) : base(br, thick, fore, fill, filltype)
        {
            fontName = font;
            fontSize = size;
            format = formatType;
            fontStyle = style;
            type = myObjectType.TEXT;
        }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            p0 = min;
            p1 = max;
            gp.Reset();
            gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            if (!isMoving)
            {
                gp.Reset();
                ctrl = new controlBoxRectangle(p0, p1);
            }
        }

        public override void draw(ref Graphics g)
        {
            g.FillPath(brush, gp);
            g.DrawPath(pen, gp);

        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override myObject clone()
        {
            myText answer = new myText();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            //answer.gp = new GraphicsPath();
            answer.contentString = contentString;
            answer.p0 = p0;
            answer.p1 = p1;
            answer.fontName = fontName;
            answer.fontSize = fontSize;
            answer.format = format;
            answer.fontStyle = fontStyle;
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.ctrl = ctrl.clone();
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(contentString);
            f.Write(Convert.ToInt32(buffer.Length));
            f.Write(buffer);
            buffer = System.Text.Encoding.Unicode.GetBytes(fontName);
            f.Write(Convert.ToInt32(buffer.Length));
            f.Write(buffer);
            f.Write(fontSize);
            f.Write(Convert.ToInt32(format));
            f.Write(Convert.ToInt32(fontStyle));
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            int count = f.ReadInt32();
            byte[] buffer = f.ReadBytes(count);
            contentString = System.Text.Encoding.Unicode.GetString(buffer);
            count = f.ReadInt32();
            buffer = f.ReadBytes(count);
            fontName = System.Text.Encoding.Unicode.GetString(buffer);
            fontSize = f.ReadSingle();
            format = (textFormat)f.ReadInt32();
            fontStyle = (FontStyle)f.ReadInt32();
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp = new GraphicsPath();
            gp.Reset();
            StringFormat temp = new StringFormat(StringFormatFlags.FitBlackBox);
            switch (format)
            {
                case textFormat.BOTTOM_VERTIAL_ALIGN:
                    temp.LineAlignment = StringAlignment.Far;
                    break;
                case textFormat.CENTER_VERTICAL_ALIGN:
                    temp.LineAlignment = StringAlignment.Center;
                    break;
                case textFormat.TOP_VERTICAL_ALIGN:
                    temp.LineAlignment = StringAlignment.Near;
                    break;
                case textFormat.CENTER_HORIZONTAL_ALIGN:
                    temp.Alignment = StringAlignment.Center;
                    break;
                case textFormat.LEFT_HORIZONTAL_ALIGN:
                    temp.Alignment = StringAlignment.Near;
                    break;
                case textFormat.RIGHT_HORIZONTAL_ALIGN:
                    temp.Alignment = StringAlignment.Far;
                    break;
            }
            gp.AddString(contentString, new FontFamily(fontName), Convert.ToInt32(fontStyle), myObject.dpiY * fontSize / 72, new Rectangle(Convert.ToInt32(p0.X), Convert.ToInt32(p0.Y), Convert.ToInt32(p1.X - p0.X), Convert.ToInt32(p1.Y - p0.Y)), temp);
            //Transform.rotate_mt = new Matrix();
            //Transform.rotate_mt.RotateAt(Convert.ToSingle(angle), ctrl.center);
            gp.Transform(Transform.rotate_mt);
        }

        public override string getText()
        {
            return contentString;
        }


        public override void setTextBox(ref TextBox t)
        {
            t.ForeColor = foregroundColor;
            t.Text = contentString;
            t.Font = new Font(new FontFamily(fontName), fontSize, fontStyle);

            //StringFormat temp = new StringFormat();
            HorizontalAlignment temp = new HorizontalAlignment();
            switch (format)
            {
                case textFormat.CENTER_HORIZONTAL_ALIGN:
                    temp = HorizontalAlignment.Center;
                    break;
                case textFormat.LEFT_HORIZONTAL_ALIGN:
                    temp = HorizontalAlignment.Left;
                    break;
                case textFormat.RIGHT_HORIZONTAL_ALIGN:
                    temp = HorizontalAlignment.Right;
                    break;
            }
            t.TextAlign = temp;
            t.Location = new Point(Convert.ToInt32(p0.X), Convert.ToInt32(p0.Y));
            t.Width = Convert.ToInt32(p1.X - p0.X);
            t.Height = Convert.ToInt32(p1.Y - p0.Y);
            //t.Focus();
        }




        public override void ChangeTextProperties(int fillStyle, Color foreColor, Color fill, FontStyle style, textFormat formatText, float size, string fontType)
        {
            if (fontStyle != style || format != formatText || fontSize != size || fontName != fontType)
            {
                fontStyle = style;
                format = formatText;
                fontSize = size;
                fontName = fontType;
                gp.Reset();
                StringFormat temp = new StringFormat(StringFormatFlags.FitBlackBox);
                switch (format)
                {
                    case textFormat.BOTTOM_VERTIAL_ALIGN:
                        temp.LineAlignment = StringAlignment.Far;
                        break;
                    case textFormat.CENTER_VERTICAL_ALIGN:
                        temp.LineAlignment = StringAlignment.Center;
                        break;
                    case textFormat.TOP_VERTICAL_ALIGN:
                        temp.LineAlignment = StringAlignment.Near;
                        break;
                    case textFormat.CENTER_HORIZONTAL_ALIGN:
                        temp.Alignment = StringAlignment.Center;
                        break;
                    case textFormat.LEFT_HORIZONTAL_ALIGN:
                        temp.Alignment = StringAlignment.Near;
                        break;
                    case textFormat.RIGHT_HORIZONTAL_ALIGN:
                        temp.Alignment = StringAlignment.Far;
                        break;
                }
                gp.AddString(contentString, new FontFamily(fontName), Convert.ToInt32(fontStyle), myObject.dpiY * fontSize / 72, new Rectangle(Convert.ToInt32(p0.X), Convert.ToInt32(p0.Y), Convert.ToInt32(p1.X - p0.X), Convert.ToInt32(p1.Y - p0.Y)), temp);
                Transform.rotate_mt = new Matrix();
                Transform.rotate_mt.RotateAt(Convert.ToSingle(angle), ctrl.center);
                gp.Transform(Transform.rotate_mt);
            }
            if (foreColor != foregroundColor)
            {
                foregroundColor = foreColor;
                createPen();
            }
            if (fill != fillColor || fillStyle != fillType)
            {
                fillType = fillStyle;
                fillColor = fill;
                createBrush();
            }
        }

        public override void ChangeText(string text)
        {
            contentString = text;
            gp.Reset();
            StringFormat temp = new StringFormat(StringFormatFlags.FitBlackBox);
            switch (format)
            {
                case textFormat.BOTTOM_VERTIAL_ALIGN:
                    temp.LineAlignment = StringAlignment.Far;
                    break;
                case textFormat.CENTER_VERTICAL_ALIGN:
                    temp.LineAlignment = StringAlignment.Center;
                    break;
                case textFormat.TOP_VERTICAL_ALIGN:
                    temp.LineAlignment = StringAlignment.Near;
                    break;
                case textFormat.CENTER_HORIZONTAL_ALIGN:
                    temp.Alignment = StringAlignment.Center;
                    break;
                case textFormat.LEFT_HORIZONTAL_ALIGN:
                    temp.Alignment = StringAlignment.Near;
                    break;
                case textFormat.RIGHT_HORIZONTAL_ALIGN:
                    temp.Alignment = StringAlignment.Far;
                    break;
            }


            gp.AddString(contentString, new FontFamily(fontName), Convert.ToInt32(fontStyle), myObject.dpiY * (fontSize / 72), new Rectangle(Convert.ToInt32(p0.X), Convert.ToInt32(p0.Y), Convert.ToInt32(p1.X - p0.X), Convert.ToInt32(p1.Y - p0.Y)), temp);
            Transform.rotate_mt = new Matrix();
            Transform.rotate_mt.RotateAt(Convert.ToSingle(angle), ctrl.center);
            gp.Transform(Transform.rotate_mt);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {

            }

            return false;
        }


    }


    class myLine : myObject
    {
        PointF p0, p1;

        public myLine() { }

        public myLine(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.LINE; }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            ctrl = new controlBoxLine();
            ctrl.load(f, angle);
            gp = new GraphicsPath();
            gp.AddLine(p0, p1);
        }

        public override myObject clone()
        {
            myLine answer = new myLine();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            //answer.gp = new GraphicsPath();
            answer.p0 = p0;
            answer.p1 = p1;
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving)
        {
            p0 = startPoint;
            p1 = endPoint;
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p0.Y == p1.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddLine(p0, p1);
            if (!isMoving)
                ctrl = new controlBoxLine(startPoint, endPoint);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }

        public override void rotate(PointF startPoint, PointF endPoint) { }

        public override bool scale(PointF startPoint, PointF endPoint) { return true; }

        public override void moveExtraPoint(PointF startPoint, PointF endPoint)
        {
            int id;

            for (id = 0; id < ctrl.cbArr.Count; id++)
            {
                if (ctrl.cbArr[id].onMouseDown(startPoint) == mouseEventType.leftClickOnExtraControlButton)
                    break;
            }
            if (id == 0)
            {
                p0 = endPoint;
                ctrl.cbArr[id].p0 = endPoint;
            }
            if (id == 1)
            {
                p1 = endPoint;
                ctrl.cbArr[id].p0 = endPoint;
            }
            gp.Reset();
            gp.AddLine(p0, p1);
        }
    }

    class myRectangle : myObject
    {
        PointF p0, p1;

        public myRectangle() { }

        public myRectangle(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.RECTANGLE; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            p0 = new PointF(min.X, min.Y);
            p1 = new PointF(max.X, max.Y);
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p0.Y == p1.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            if (!isMoving)
                ctrl = new controlBoxRectangle(min, max);
        }

        public override myObject clone()
        {
            myRectangle answer = new myRectangle();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            //answer.gp = new GraphicsPath();
            answer.p0 = p0;
            answer.p1 = p1;
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.FillPath(brush, gp);
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.arrPoint = new PointF[] { p0, p1 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;
            }
            return false;

        }
    }

    class myParallelogram : myObject
    {
        PointF p0, p1, p2, p3;

        public myParallelogram() { }

        public myParallelogram(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.PARALLELOGRAM; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            p0 = new PointF(Convert.ToSingle(min.X + 0.2 * (max.X - min.X)), min.Y);
            p1 = new PointF(max.X, min.Y);
            p2 = new PointF(Convert.ToSingle(max.X - 0.2 * (max.X - min.X)), max.Y);
            p3 = new PointF(min.X, max.Y);
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p0.Y == p1.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddPolygon(new PointF[] { p0, p1, p2, p3 });
            if (!isMoving)
                ctrl = new controlBoxRectangle(min, max);
        }

        public override myObject clone()
        {
            myParallelogram answer = new myParallelogram();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            //answer.gp = new GraphicsPath();
            answer.p0 = p0;
            answer.p1 = p1;
            answer.p2 = p2;
            answer.p3 = p3;
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            f.Write(p2.Y);
            f.Write(p2.Y);
            f.Write(p3.Y);
            f.Write(p3.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            p2.X = f.ReadSingle();
            p2.Y = f.ReadSingle();
            p3.X = f.ReadSingle();
            p3.Y = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] { p0, p1, p2, p3 });
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.FillPath(brush, gp);
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1, p2, p3 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
            p2 = Transform.arrPoint[2];
            p3 = Transform.arrPoint[3];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.arrPoint = new PointF[] { p0, p1, p2, p3 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                p2 = Transform.arrPoint[2];
                p3 = Transform.arrPoint[3];
                return true;
            }
            return false;
        }
    }

    class myPolygon : myObject
    {
        List<PointF> p;
        public myPolygon() { }

        public myPolygon(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.POLYGON; }

        public override void adjust(List<PointF> lsPoint, bool isMoving = false)
        {
            p = new List<PointF>(lsPoint);
            for (int i = 0; i < lsPoint.Count; i++)
                p.Add(lsPoint[i]);
            gp.Reset();
            gp.AddLines(p.ToArray());
            if (!isMoving)
            {
                float xmin = 999999, ymin = 999999, xmax = -99999999, ymax = -99999999;
                for (int i = 0; i < p.Count; i++)
                {
                    if (p[i].X < xmin)
                        xmin = p[i].X;
                    if (p[i].Y < ymin)
                        ymin = p[i].Y;
                    if (p[i].X > xmax)
                        xmax = p[i].X;
                    if (p[i].Y > ymax)
                        ymax = p[i].Y;
                }
                ctrl = new controlBoxRectangle(new PointF(xmin, ymin), new PointF(xmax, ymax));
            }
        }

        public override myObject clone()
        {
            myPolygon answer = new myPolygon();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p = new List<PointF>();
            for (int i = 0; i < p.Count; i++)
                answer.p.Add(p[i]);
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p.Count);
            for (int i = 0; i < p.Count; i++)
            {
                f.Write(p[i].X);
                f.Write(p[i].Y);
            }

            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            PointF temp;
            p = new List<PointF>();
            int count = f.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                temp = new PointF();
                temp.X = f.ReadSingle();
                temp.Y = f.ReadSingle();
                p.Add(temp);
            }
            gp = new GraphicsPath();
            gp.AddLines(p.ToArray());
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.FillPath(brush, gp);
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = p.ToArray();
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            for (int i = 0; i < p.Count; i++)
                p[i] = Transform.arrPoint[i];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.lsPoint = new List<PointF>();
                for (int i = 0; i < p.Count; i++)
                    Transform.lsPoint.Add(p[i]);
                Transform.arrPoint = Transform.lsPoint.ToArray();
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                for (int i = 0; i < p.Count; i++)
                    p[i] = Transform.lsPoint[i];
                return true;
            }
            return false;
        }


    }

    class myBrokenLine : myObject
    {
        List<PointF> p;
        //controlBoxLine ctrl;
        public myBrokenLine() { }

        public myBrokenLine(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.BROKENLINE; }

        public override void adjust(List<PointF> lsPoint, bool isMoving = false)
        {
            p = new List<PointF>();
            for (int i = 0; i < lsPoint.Count; i++)
                p.Add(lsPoint[i]);
            gp.Reset();
            gp.AddLines(p.ToArray());
            if (!isMoving)
            {
                float xmin = 999999, ymin = 999999, xmax = -99999999, ymax = -99999999;
                for (int i = 0; i < p.Count; i++)
                {
                    if (p[i].X < xmin)
                        xmin = p[i].X;
                    if (p[i].Y < ymin)
                        ymin = p[i].Y;
                    if (p[i].X > xmax)
                        xmax = p[i].X;
                    if (p[i].Y > ymax)
                        ymax = p[i].Y;
                }
                ctrl = new controlBoxRectangle(new PointF(xmin, ymin), new PointF(xmax, ymax));
            }
        }

        public override myObject clone()
        {
            myBrokenLine answer = new myBrokenLine();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p = new List<PointF>();
            for (int i = 0; i < p.Count; i++)
                answer.p.Add(new PointF(p[i].X, p[i].Y));
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p.Count);
            for (int i = 0; i < p.Count; i++)
            {
                f.Write(p[i].X);
                f.Write(p[i].Y);
            }

            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            PointF temp;
            p = new List<PointF>();
            int count = f.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                temp = new PointF();
                temp.X = f.ReadSingle();
                temp.Y = f.ReadSingle();
                p.Add(temp);
            }
            gp = new GraphicsPath();
            gp.AddLines(p.ToArray());
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = p.ToArray();
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            for (int i = 0; i < p.Count; i++)
                p[i] = Transform.arrPoint[i];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.lsPoint = new List<PointF>();
                for (int i = 0; i < p.Count; i++)
                    Transform.lsPoint.Add(p[i]);
                Transform.arrPoint = Transform.lsPoint.ToArray();
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                for (int i = 0; i < p.Count; i++)
                    p[i] = Transform.lsPoint[i];
                return true;
            }
            return false;
        }
    }

    class myCircle : myObject
    {
        PointF p0, p1;
        //controlBoxRectangle ctrl;

        public myCircle() { }

        public myCircle(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.CIRCLE; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            //PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            float Dist = Math.Min(Math.Abs(endPoint.X - startPoint.X), Math.Abs(endPoint.Y - startPoint.Y));
            if (startPoint.X < endPoint.X)
            {
                p0.X = startPoint.X;
                p1.X = startPoint.X + Dist;
            }
            else
            {
                p0.X = startPoint.X - Dist;
                p1.X = startPoint.X;
            }
            if (startPoint.Y < endPoint.Y)
            {
                p0.Y = startPoint.Y;
                p1.Y = startPoint.Y + Dist;
            }
            else
            {
                p0.Y = startPoint.Y - Dist;
                p1.Y = startPoint.Y;
            }
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p0.Y == p1.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddEllipse(p0.X, p0.Y, p1.X - p0.X, p1.X - p0.X);
            if (!isMoving)
                ctrl = new controlBoxRectangle(p0, p1);
        }

        public override myObject clone()
        {
            myCircle answer = new myCircle();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddEllipse(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }


        public override void draw(ref Graphics g)
        {
            g.FillPath(brush, gp);
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }


        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {

                Transform.arrPoint = new PointF[] { p0, p1 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;

            }
            return false;

        }
    }

    class myCircleArc : myObject
    {
        PointF p0, p1;
        float startAngle = 90, sweeepAngle = 180;
        //controlBoxLine ctrl;
        public myCircleArc() { }

        public myCircleArc(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype)
        {
            type = myObjectType.CIRCLEARC;
        }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            float Dist = Math.Min(Math.Abs(endPoint.X - startPoint.X), Math.Abs(endPoint.Y - startPoint.Y));
            if (startPoint.X < endPoint.X)
            {
                p0.X = startPoint.X;
                p1.X = startPoint.X + Dist;
            }
            else
            {
                p0.X = startPoint.X - Dist;
                p1.X = startPoint.X;
            }
            if (startPoint.Y < endPoint.Y)
            {
                p0.Y = startPoint.Y;
                p1.Y = startPoint.Y + Dist;
            }
            else
            {
                p0.Y = startPoint.Y - Dist;
                p1.Y = startPoint.Y;
            }
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p0.Y == p1.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddArc(p0.X, p0.Y, p1.X - p0.X, p1.X - p0.X, startAngle, sweeepAngle);
            if (!isMoving)
                ctrl = new controlBoxRectangle(p0, p1);
        }

        public override myObject clone()
        {
            myCircleArc answer = new myCircleArc();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;
            answer.startAngle = startAngle;
            answer.sweeepAngle = sweeepAngle;

            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            f.Write(startAngle);
            f.Write(sweeepAngle);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            startAngle = f.ReadSingle();
            sweeepAngle = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddArc(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y, startAngle, sweeepAngle);
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }


        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.arrPoint = new PointF[] { p0, p1 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;
            }
            return false;
        }
    }

    class myEllipse : myObject
    {
        PointF p0, p1;

        public myEllipse() { }

        public myEllipse(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.ELLIPSE; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            p0 = min;
            p1 = max;
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p1.Y == p0.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddEllipse(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
            if (!isMoving)
                ctrl = new controlBoxRectangle(min, max);
        }

        public override myObject clone()
        {
            myEllipse answer = new myEllipse();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddEllipse(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {

            g.FillPath(brush, gp);
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }


        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.arrPoint = new PointF[] { p0, p1 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;
            }
            return false;
        }
    }

    class myEllipseArc : myObject
    {

        PointF p0, p1;
        float startAngle = 90, sweeepAngle = 180;
        //float a, b;
        //controlBoxLine ctrl;
        public myEllipseArc() { }

        public myEllipseArc(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.ELLIPSEARC; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            p0 = min;
            p1 = max;
            if (p0.X == p1.X)
                p0.X -= 2;
            if (p0.Y == p1.Y)
                p0.Y -= 2;
            gp.Reset();
            gp.AddArc(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y, startAngle, sweeepAngle);
            if (!isMoving)
                ctrl = new controlBoxRectangle(min, max);
        }

        public override myObject clone()
        {
            myEllipseArc answer = new myEllipseArc();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;
            answer.startAngle = startAngle;
            answer.sweeepAngle = sweeepAngle;
            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            answer.createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            f.Write(startAngle);
            f.Write(sweeepAngle);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            startAngle = f.ReadSingle();
            sweeepAngle = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddArc(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y, startAngle, sweeepAngle);
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }


        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                Transform.arrPoint = new PointF[] { p0, p1 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;
            }
            return false;
        }
    }


    class myParabola : myObject
    {
        PointF p0, p1;
        //controlBoxLine ctrl;
        public myParabola() { }

        public myParabola(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.PARABOLA; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            if (min.X == max.X)
                min.X -= 2;
            if (min.Y == max.Y)
                min.Y -= 2;
            List<PointF> ls = new List<PointF>();
            float a = (max.Y - min.Y) / (((max.X - min.X) / 2) * ((max.X - min.X) / 2)), center=(min.X+max.X)/2;
            double temp = Math.Ceiling(Convert.ToDouble(1 / (2 * a)));
            float xCor = 0, yCor = 0;
            for (; xCor <= temp && xCor < (max.X - min.X) / 2 && yCor < (max.Y - min.Y); xCor++)
            {
                yCor = a * xCor * xCor;
                ls.Add(new PointF(center+xCor, max.Y - yCor));
                ls.Insert(0, new PointF(center - xCor, max.Y - yCor));
            }
            xCor--;

            for (; yCor < (max.Y-min.Y) && xCor < (max.X - min.X) / 2; yCor++)
            {
                xCor = Convert.ToSingle(Math.Sqrt(yCor/a));
                ls.Add(new PointF(center + xCor, max.Y - yCor));
                ls.Insert(0, new PointF(center - xCor, max.Y - yCor));
            }
            p0 = min;
            p1 = max;
            gp.Reset();
            gp.AddLines(ls.ToArray());
            if (!isMoving)
            {
                ctrl = new controlBoxRectangle(min, max);
            }
        }

        public override myObject clone()
        {
            myParabola answer = new myParabola();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;

            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            gp = new GraphicsPath();
            List<PointF> ls = new List<PointF>();
            float a = (p1.Y - p0.Y) / (((p1.X - p0.X) / 2) * ((p1.X - p0.X) / 2)), center = (p1.X + p0.X) / 2;
            double temp = Math.Ceiling(Convert.ToDouble(1 / (2 * a)));
            float xCor = 0, yCor = 0;
            for (; xCor <= temp && xCor < (p1.X - p0.X) / 2 && yCor < (p1.Y - p0.Y); xCor++)
            {
                yCor = a * xCor * xCor;
                ls.Add(new PointF(center + xCor, p1.Y - yCor));
                ls.Insert(0, new PointF(center - xCor, p1.Y - yCor));
            }
            xCor--;

            for (; yCor < (p1.Y - p0.Y) && xCor < (p1.X - p0.X) / 2; yCor++)
            {
                xCor = Convert.ToSingle(Math.Sqrt(yCor / a));
                ls.Add(new PointF(center + xCor, p1.Y - yCor));
                ls.Insert(0, new PointF(center - xCor, p1.Y - yCor));
            }
            gp.AddLines(ls.ToArray());
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {

                Transform.arrPoint = new PointF[] { p0, p1};
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;

            }
            return false;
        }
    }

    class myBezierCurve : myObject
    {
        PointF p0, p1, p2, p3;
        //controlBoxLine ctrl;
        public myBezierCurve() { }

        public myBezierCurve(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.BEZIERCURVE; }

        public override void adjust(List<PointF> lsPoint, bool isMoving = false)
        {
            if (lsPoint.Count > 0)
                p0 = lsPoint[0];
            if (lsPoint.Count > 1)
                p3 = lsPoint[1];
            else
                p3 = p0;
            if (lsPoint.Count > 2)
                p1 = lsPoint[2];
            else
                p1 = p0;
            if (lsPoint.Count > 3)
                p2 = lsPoint[3];
            else
                p2 = p1;
            gp.Reset();
            gp.AddBezier(p0, p1, p2, p3);
            if (!isMoving)
            {
                PointF min = new PointF(Math.Min(Math.Min(p0.X, p1.X), Math.Min(p2.X, p3.X)), Math.Min(Math.Min(p0.Y, p1.Y), Math.Min(p2.Y, p3.Y)));
                PointF max = new PointF(Math.Max(Math.Max(p0.X, p1.X), Math.Max(p2.X, p3.X)), Math.Max(Math.Max(p0.Y, p1.Y), Math.Max(p2.Y, p3.Y)));
                ctrl = new controlBoxRectangle(min, max);
            }
        }

        public override myObject clone()
        {
            myBezierCurve answer = new myBezierCurve();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;
            answer.p2 = p2;
            answer.p3 = p3;

            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            f.Write(p2.X);
            f.Write(p2.Y);
            f.Write(p3.X);
            f.Write(p3.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            p2.X = f.ReadSingle();
            p2.Y = f.ReadSingle();
            p3.X = f.ReadSingle();
            p3.Y = f.ReadSingle();
            gp = new GraphicsPath();
            gp.AddBezier(p0, p1, p2, p3);
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1, p2, p3 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
            p2 = Transform.arrPoint[2];
            p3 = Transform.arrPoint[3];
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {

                Transform.arrPoint = new PointF[] { p0, p1, p2, p3 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                p2 = Transform.arrPoint[2];
                p3 = Transform.arrPoint[3];
                return true;

            }
            return false;
        }
    }

    class myHypebola : myObject
    {
        PointF p0, p1;
        GraphicsPath gp2 = new GraphicsPath();
        //controlBoxLine ctrl;
        public myHypebola() { }

        public myHypebola(outlineStyle br, int thick, Color fore, Color fill, int filltype) : base(br, thick, fore, fill, filltype) { type = myObjectType.HYPEBOLA; }

        public override void adjust(PointF startPoint, PointF endPoint, bool isMoving = false)
        {
            PointF min = new PointF(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y)), max = new PointF(Math.Max(startPoint.X, endPoint.X), Math.Max(startPoint.Y, endPoint.Y));
            if (min.X == max.X)
                min.X -= 2;
            if (min.Y == max.Y)
                min.Y -= 2;
            List<PointF> ls = new List<PointF>(), ls2=new List<PointF>();
            float xCor = 0, yCor = 0, a=(max.X-min.X)/20, b=(max.Y-min.Y)/20, centerX=(max.X+min.X)/2, centerY=(max.Y+min.Y)/2;
            float d = (a) / b, square;
            float squareA = a * a, squareB = b * b;
            square = squareB;
            xCor = a; yCor = 0;
            for (; xCor * squareB >= yCor * squareA && yCor < (max.Y-min.Y)/2 && xCor < (max.X-min.X)/2; yCor++)
            {
                xCor = d * Convert.ToSingle(Math.Sqrt(square + yCor * yCor));
                ls.Add(new PointF(centerX + xCor, centerY - yCor));
                ls.Insert(0, new PointF(centerX + xCor, centerY + yCor));
                ls2.Add(new PointF(centerX - xCor, centerY - yCor));
                ls2.Insert(0, new PointF(centerX - xCor, centerY + yCor));
            }
            yCor--;
            square = squareA; d = (b) / a;
            if (b <= a)
            {
                for (; xCor < (max.X-min.X)/2 && yCor < (max.Y-min.Y)/2; xCor++)
                {
                    yCor = d*Convert.ToSingle(  Math.Sqrt(xCor * xCor - square));
                    ls.Add(new PointF(centerX + xCor, centerY - yCor));
                    ls.Insert(0, new PointF(centerX + xCor, centerY + yCor));
                    ls2.Add(new PointF(centerX - xCor, centerY - yCor));
                    ls2.Insert(0, new PointF(centerX - xCor, centerY + yCor));

                }
            }
            
            
            p0 = min;
            p1 = max;
            gp.Reset();
            gp.AddLines(ls.ToArray());
            gp2.Reset();
            gp2.AddLines(ls2.ToArray());
            if (!isMoving)
            {
                ctrl = new controlBoxRectangle(min, max);
            }
        }

        public override myObject clone()
        {
            myHypebola answer = new myHypebola();
            answer.outline = outline;
            answer.fillType = fillType;
            answer.brushThickness = brushThickness;
            answer.foregroundColor = foregroundColor;
            answer.fillColor = fillColor;
            answer.p0 = p0;
            answer.p1 = p1;

            answer.ctrl = ctrl.clone();
            answer.gp = (GraphicsPath)gp.Clone();
            answer.gp2 = (GraphicsPath)gp2.Clone();
            answer.angle = angle;
            answer.type = type;
            answer.createPen();
            createBrush();
            return answer;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            ctrl.save(f);

        }

        public override void load(BinaryReader f)
        {
            base.load(f);
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            gp = new GraphicsPath();
            gp2 = new GraphicsPath();
            List<PointF> ls = new List<PointF>(), ls2 = new List<PointF>();
            float xCor = 0, yCor = 0, a = (1 / 20) * (p1.X - p0.X), b = 1 / 20 * (p1.Y - p0.Y), centerX = (p1.X + p0.X) / 2, centerY = (p1.Y + p0.Y) / 2;
            float d = (a) / b, square;
            float squareA = a * a, squareB = b * b;
            square = squareB;
            xCor = a; yCor = 0;
            for (; xCor * squareB >= yCor * squareA && yCor < (p1.Y - p0.Y) / 2 && xCor < (p1.X - p0.X) / 2; yCor++)
            {
                xCor = d * Convert.ToSingle(Math.Sqrt(square + yCor * yCor));
                ls.Add(new PointF(centerX + xCor, centerY - yCor));
                ls.Insert(0, new PointF(centerX + xCor, centerY + yCor));
                ls2.Add(new PointF(centerX - xCor, centerY - yCor));
                ls2.Insert(0, new PointF(centerX - xCor, centerY + yCor));
            }
            yCor--;
            square = squareA; d = (b) / a;
            if (b <= a)
            {
                for (; xCor < (p1.X - p0.X) / 2 && yCor < (p1.Y - p0.Y) / 2; xCor++)
                {
                    yCor = d * Convert.ToSingle(Math.Sqrt(xCor * xCor - square));
                    ls.Add(new PointF(centerX + xCor, centerY - yCor));
                    ls.Insert(0, new PointF(centerX + xCor, centerY + yCor));
                    ls2.Add(new PointF(centerX - xCor, centerY - yCor));
                    ls2.Insert(0, new PointF(centerX - xCor, centerY + yCor));

                }
            }
            
            gp.Reset();
            gp.AddLines(ls.ToArray());
            gp2.Reset();
            gp2.AddLines(ls2.ToArray());
            ctrl = new controlBoxRectangle();
            ctrl.load(f, angle);
            gp.Transform(Transform.rotate_mt);
            gp2.Transform(Transform.rerotate_mt);
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(pen, gp);
            g.DrawPath(pen, gp2);
        }

        public override void drawControlBox(ref Graphics g)
        {
            ctrl.draw(ref g);
        }

        public override void translate(PointF startPoint, PointF endPoint)
        {
            base.translate(startPoint, endPoint);
            Transform.arrPoint = new PointF[] { p0, p1 };
            Transform.translate_mt.TransformPoints(Transform.arrPoint);
            p0 = Transform.arrPoint[0];
            p1 = Transform.arrPoint[1];
            gp2.Transform(Transform.translate_mt);
        }

        public override void rotate(PointF startPoint, PointF endPoint)
        {
            base.rotate(startPoint, endPoint);
            gp2.Transform(Transform.rotate_mt);
        }

        public override bool scale(PointF startPoint, PointF endPoint)
        {
            if (base.scale(startPoint, endPoint))
            {
                gp2.Transform(Transform.temporary);
                Transform.arrPoint = new PointF[] { p0, p1 };
                Transform.temporary1.TransformPoints(Transform.arrPoint);
                Transform.temporary2.TransformPoints(Transform.arrPoint);
                p0 = Transform.arrPoint[0];
                p1 = Transform.arrPoint[1];
                return true;

            }
            return false;
        }
    }
    //public onMouseDown()

}
