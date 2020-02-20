using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Design;
using System.IO;

namespace myPaint
{

    enum Type_ControlButton { topZoom, bottomZoom, leftZoom, rightZoom, topLeftZoom, topRightZoom, bottomLeftZoom, bottomRightZoom, rotate, extra};

    class controlBox
    {
        protected static Pen controlBoxPen = new Pen(Color.FromArgb(255, 180, 180, 180), 1);
        public PointF center;
        public GraphicsPath gp;
        public List<controlButton> cbArr;
        public PointF p0, p1;


        public controlBox()
        {
            controlBoxPen.DashStyle = DashStyle.Dash;
            gp = new GraphicsPath();
            cbArr = new List<controlButton>();
        }
        public virtual void draw(ref Graphics g) { }

        public virtual void save(BinaryWriter f) {
            f.Write(p0.X);
            f.Write(p0.Y);
            f.Write(p1.X);
            f.Write(p1.Y);
            f.Write(Convert.ToInt32(cbArr.Count));
            for(int i = 0; i < cbArr.Count; i++)
            {
                f.Write(Convert.ToInt32(cbArr[i].type));
                f.Write(cbArr[i].p0.X);
                f.Write(cbArr[i].p0.Y);
            }

        }

        public virtual controlBox clone()
        {
            return new controlBox();
        }

        public virtual void load(BinaryReader f, double angle) {
            p0 = new PointF();
            p1 = new PointF();
            center = new PointF();
            p0.X = f.ReadSingle();
            p0.Y = f.ReadSingle();
            p1.X = f.ReadSingle();
            p1.Y = f.ReadSingle();
            center.X = (p0.X + p1.X) / 2;
            center.Y = (p0.Y + p1.Y) / 2;
            int count = f.ReadInt32();
            Type_ControlButton type;
            PointF p = new PointF();
            for(int i = 0; i < count; i++)
            {
                type = (Type_ControlButton)f.ReadInt32();
                p.X = f.ReadSingle();
                p.Y = f.ReadSingle();
                switch (type) {
                    case Type_ControlButton.rotate:
                        cbArr.Add(new rotateButton(p, type));
                        break;
                    case Type_ControlButton.extra:
                        cbArr.Add(new extraButton(p, type));
                        break;
                    default:
                        cbArr.Add(new zoomButton(p, type));
                        break;
                }
            }
        }

        public virtual mouseEventType onMouseDown(PointF p)
        {
            return mouseEventType.leftClickOnBlankSpace;
        }

        public virtual mouseEventType onMouseMove(PointF p)
        {
            return mouseEventType.overOnBlankSpace;
        }
    }

    class controlBoxLine : controlBox
    {
        public controlBoxLine() : base()
        {

        }

        public override controlBox clone()
        {
            controlBoxLine answer=new controlBoxLine();
            answer.p0 = p0;
            answer.p1 = p1;
            answer.center = center;
            answer.gp = (GraphicsPath)gp.Clone();
            answer.cbArr = new List<controlButton>();
            answer.cbArr.Add(new extraButton(new PointF(0, 0), Type_ControlButton.extra));
            answer.cbArr[0].p0 = cbArr[0].p0;
            answer.cbArr.Add(new extraButton(new PointF(0, 0), Type_ControlButton.extra));
            answer.cbArr[1].p0 = cbArr[1].p0;
            return answer;
        }

        public controlBoxLine(PointF startPointF,PointF endPointF):base()
        {
            p0 = startPointF;
            p1 = endPointF;
            cbArr.Add(new extraButton(p0, Type_ControlButton.extra));
            cbArr.Add(new extraButton(p1, Type_ControlButton.extra));
        }

        public override void draw(ref Graphics g)
        {
            for (int i = 0; i < cbArr.Count; i++)
                cbArr[i].draw(ref g);
        }

        public override mouseEventType onMouseDown(PointF p)
        {
            for (int i = 0; i < cbArr.Count; i++)
                if (cbArr[i].onMouseDown(p) != mouseEventType.leftClickOnBlankSpace)
                    return cbArr[i].onMouseDown(p);
            return mouseEventType.leftClickOnBlankSpace;
        }

        public override mouseEventType onMouseMove(PointF p)
        {
            for (int i = 0; i < cbArr.Count; i++)
                if (cbArr[i].onMouseMove(p) != mouseEventType.overOnBlankSpace)
                    return cbArr[i].onMouseMove(p);
            return mouseEventType.overOnBlankSpace;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
        }

        public override void load(BinaryReader f, double angle)
        {
            base.load(f, angle);
        }

    }

    class controlBoxRectangle : controlBox
    {
        public controlBoxRectangle() : base() { }

        public controlBoxRectangle(PointF startPointF, PointF endPointF):base()
        {
            gp = new GraphicsPath();
            cbArr = new List<controlButton>();
            p0 = startPointF;
            p1 = endPointF;
            center = new PointF((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            cbArr.Add(new zoomButton(p0, Type_ControlButton.topLeftZoom));
            cbArr.Add(new zoomButton(new PointF((p0.X + p1.X) / 2, p0.Y), Type_ControlButton.topZoom));
            cbArr.Add(new zoomButton(new PointF(p1.X,p0.Y), Type_ControlButton.topRightZoom));
            cbArr.Add(new zoomButton(new PointF(p1.X, (p0.Y+p1.Y)/2), Type_ControlButton.rightZoom));
            cbArr.Add(new zoomButton(p1, Type_ControlButton.bottomRightZoom));
            cbArr.Add(new zoomButton(new PointF((p0.X + p1.X) / 2, p1.Y), Type_ControlButton.bottomZoom));
            cbArr.Add(new zoomButton(new PointF(p0.X,p1.Y), Type_ControlButton.bottomLeftZoom));
            cbArr.Add(new zoomButton(new PointF(p0.X, (p0.Y+p1.Y)/2), Type_ControlButton.leftZoom));
            cbArr.Add(new rotateButton(new PointF(center.X, p0.Y - 10), Type_ControlButton.rotate));
            gp.AddPolygon(new PointF[] { p0,new PointF(p1.X,p0.Y),p1,new PointF(p0.X,p1.Y) });
        }

        public override controlBox clone()
        {
            controlBoxRectangle answer = new controlBoxRectangle();
            answer.p0 = p0;
            answer.p1 = p1;
            answer.center = center;
            answer.gp = (GraphicsPath)gp.Clone();
            answer.cbArr = new List<controlButton>();
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.topLeftZoom));
            answer.cbArr[0].p0 = cbArr[0].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.topZoom));
            answer.cbArr[1].p0 = cbArr[1].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.topRightZoom));
            answer.cbArr[2].p0 = cbArr[2].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.rightZoom));
            answer.cbArr[3].p0 = cbArr[3].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.bottomRightZoom));
            answer.cbArr[4].p0 = cbArr[4].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.bottomZoom));
            answer.cbArr[5].p0 = cbArr[5].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.bottomLeftZoom));
            answer.cbArr[6].p0 = cbArr[6].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0,0), Type_ControlButton.leftZoom));
            answer.cbArr[7].p0 = cbArr[7].p0;
            answer.cbArr.Add(new rotateButton(new PointF(0,0), Type_ControlButton.rotate));
            answer.cbArr[8].p0 = cbArr[8].p0;//gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            return answer;
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(controlBox.controlBoxPen, gp);
            for (int i = 0; i < cbArr.Count; i++)
                cbArr[i].draw(ref g);
        }

        public override mouseEventType onMouseDown(PointF p)
        {
            for (int i = 0; i < cbArr.Count; i++)
                if (cbArr[i].onMouseDown(p) != mouseEventType.leftClickOnBlankSpace)
                    return cbArr[i].onMouseDown(p);
            if (gp.IsVisible(p))
                return mouseEventType.leftClickInControlBox;
            return mouseEventType.leftClickOnBlankSpace;
        }

        public override mouseEventType onMouseMove(PointF p)
        {
            for (int i = 0; i < cbArr.Count; i++)
                if (cbArr[i].onMouseMove(p) != mouseEventType.overOnBlankSpace)
                    return cbArr[i].onMouseMove(p);
            if (gp.IsVisible(p))
                return mouseEventType.overInControlBox;
            return mouseEventType.overOnBlankSpace;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
        }

        public override void load(BinaryReader f, double angle)
        {
            base.load(f, angle);
            gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            Transform.rotate_mt = new Matrix();
            Transform.rotate_mt.RotateAt(Convert.ToSingle(angle), center);
            gp.Transform(Transform.rotate_mt);
        }

    }

    class controlBoxParalellogram: controlBox
    {
        public controlBoxParalellogram() : base() { }

        public controlBoxParalellogram(PointF startPointF, PointF endPointF) : base()
        {
            gp = new GraphicsPath();
            cbArr = new List<controlButton>();
            p0 = startPointF;
            p1 = endPointF;
            center = new PointF((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            cbArr.Add(new zoomButton(p0, Type_ControlButton.topLeftZoom));
            cbArr.Add(new zoomButton(new PointF((p0.X + p1.X) / 2, p0.Y), Type_ControlButton.topZoom));
            cbArr.Add(new zoomButton(new PointF(p1.X, p0.Y), Type_ControlButton.topRightZoom));
            cbArr.Add(new zoomButton(new PointF(p1.X, (p0.Y + p1.Y) / 2), Type_ControlButton.rightZoom));
            cbArr.Add(new zoomButton(p1, Type_ControlButton.bottomRightZoom));
            cbArr.Add(new zoomButton(new PointF((p0.X + p1.X) / 2, p1.Y), Type_ControlButton.bottomZoom));
            cbArr.Add(new zoomButton(new PointF(p0.X, p1.Y), Type_ControlButton.bottomLeftZoom));
            cbArr.Add(new zoomButton(new PointF(p0.X, (p0.Y + p1.Y) / 2), Type_ControlButton.leftZoom));
            cbArr.Add(new rotateButton(new PointF(center.X, p0.Y - 10), Type_ControlButton.rotate));
            cbArr.Add(new extraButton(new PointF(startPointF.X + Convert.ToSingle(0.2) * (endPointF.X - startPointF.X), startPointF.Y), Type_ControlButton.extra));
            cbArr.Add(new extraButton(new PointF(endPointF.X - Convert.ToSingle(0.2) * (endPointF.X - startPointF.X),endPointF.Y), Type_ControlButton.extra));
            gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
        }

        public override controlBox clone()
        {
            controlBoxParalellogram answer = new controlBoxParalellogram();
            answer.p0 = p0;
            answer.p1 = p1;
            answer.center = center;
            answer.gp = (GraphicsPath)gp.Clone();
            answer.cbArr = new List<controlButton>();
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.topLeftZoom));
            answer.cbArr[0].p0 = cbArr[0].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.topZoom));
            answer.cbArr[1].p0 = cbArr[1].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.topRightZoom));
            answer.cbArr[2].p0 = cbArr[2].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.rightZoom));
            answer.cbArr[3].p0 = cbArr[3].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.bottomRightZoom));
            answer.cbArr[4].p0 = cbArr[4].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.bottomZoom));
            answer.cbArr[5].p0 = cbArr[5].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.bottomLeftZoom));
            answer.cbArr[6].p0 = cbArr[6].p0;
            answer.cbArr.Add(new zoomButton(new PointF(0, 0), Type_ControlButton.leftZoom));
            answer.cbArr[7].p0 = cbArr[7].p0;
            answer.cbArr.Add(new rotateButton(new PointF(0, 0), Type_ControlButton.rotate));
            answer.cbArr[8].p0 = cbArr[8].p0;//gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            answer.cbArr.Add(new extraButton(new PointF(0,0), Type_ControlButton.extra));
            answer.cbArr[9].p0 = cbArr[9].p0;
            answer.cbArr.Add(new extraButton(new PointF(0,0), Type_ControlButton.extra));
            answer.cbArr[10].p0 = cbArr[10].p0;
            return answer;
        }

        public override void draw(ref Graphics g)
        {
            g.DrawPath(controlBox.controlBoxPen, gp);
            for (int i = 0; i < cbArr.Count; i++)
                cbArr[i].draw(ref g);
        }

        public override mouseEventType onMouseDown(PointF p)
        {
            for (int i = 0; i < cbArr.Count; i++)
                if (cbArr[i].onMouseDown(p) != mouseEventType.leftClickOnBlankSpace)
                    return cbArr[i].onMouseDown(p);
            if (gp.IsVisible(p))
                return mouseEventType.leftClickInControlBox;
            return mouseEventType.leftClickOnBlankSpace;
        }

        public override mouseEventType onMouseMove(PointF p)
        {
            for (int i = 0; i < cbArr.Count; i++)
                if (cbArr[i].onMouseMove(p) != mouseEventType.overOnBlankSpace)
                    return cbArr[i].onMouseMove(p);
            if (gp.IsVisible(p))
                return mouseEventType.overInControlBox;
            return mouseEventType.overOnBlankSpace;
        }

        public override void save(BinaryWriter f)
        {
            base.save(f);
        }

        public override void load(BinaryReader f, double angle)
        {
            base.load(f, angle);
            gp.AddPolygon(new PointF[] { p0, new PointF(p1.X, p0.Y), p1, new PointF(p0.X, p1.Y) });
            Transform.rotate_mt = new Matrix();
            Transform.rotate_mt.RotateAt(Convert.ToSingle(angle), center);
            gp.Transform(Transform.rotate_mt);
        }

    }




    class controlButton
    {
        public PointF p0;
        protected int r = 3;
        public Type_ControlButton type;

        virtual public mouseEventType onMouseDown(PointF p) { return mouseEventType.leftClickOnBlankSpace; }
        virtual public mouseEventType onMouseMove(PointF p) { return mouseEventType.leftClickOnBlankSpace; }
        virtual public void draw(ref Graphics g) { }
        virtual public void setPosition(int x, int y) { p0.X = x; p0.Y = y; }
        
        
        public PointF get() { return p0; }
        public void set(PointF p) { p0.X = p.X; p0.Y = p.Y; }
    }

    class rotateButton : controlButton
    {
        static Pen rotateButtonPen = new Pen(Color.FromArgb(255, 250, 100, 100), 1);
        static Brush rotateButtonBrush = Brushes.HotPink;
        public rotateButton(PointF p, Type_ControlButton t)
        {
            p0 = p;
            type = t;
            r = 4;
        }

        public override mouseEventType onMouseDown(PointF p)
        {
            if ((Math.Abs(p.X - p0.X) <= r && Math.Abs(p.Y - p0.Y) <= r))
                return mouseEventType.leftClickOnRotateControlButton;
            else
                return mouseEventType.leftClickOnBlankSpace;
        }

        public override mouseEventType onMouseMove(PointF p)
        {
            if ((Math.Abs(p.X - p0.X) <= r && Math.Abs(p.Y - p0.Y) <= r))
                return mouseEventType.overOnRotateControlButton;
            else
                return mouseEventType.overOnBlankSpace;
        }

        public override void draw(ref Graphics g)
        {
            g.FillEllipse(rotateButtonBrush, new RectangleF(p0.X - r, p0.Y - r, r << 1, r << 1));
            g.DrawEllipse(rotateButtonPen, new RectangleF(p0.X - r, p0.Y - r, r << 1, r << 1));
        }

        public override void setPosition(int x, int y)
        {
            base.setPosition(x, y);
        }


    }

    class zoomButton : controlButton
    {
        static Pen zoomButtonPen = new Pen(Color.FromArgb(255, 80, 80, 80), 1);
        static Brush zoomButtonBrush = Brushes.White;
        public zoomButton(PointF p, Type_ControlButton t)
        {
            p0 = p;
            type = t;
        }


        public override void draw(ref Graphics g)
        {
            g.FillEllipse(zoomButtonBrush, new RectangleF(p0.X - r, p0.Y - r, r << 1, r << 1));
            g.DrawEllipse(zoomButtonPen,new RectangleF(p0.X - r, p0.Y - r, r << 1, r << 1));
        }

        public override void setPosition(int x, int y)
        {
            base.setPosition(x, y);
        }

        public override mouseEventType onMouseDown(PointF p)
        {
            if ((Math.Abs(p.X - p0.X) <= r && Math.Abs(p.Y - p0.Y) <= r))
                return mouseEventType.leftClickOnZoomControlButton;
            else
                return mouseEventType.leftClickOnBlankSpace;
        }

        public override mouseEventType onMouseMove(PointF p)
        {
            if ((Math.Abs(p.X - p0.X) <= r && Math.Abs(p.Y - p0.Y) <= r))
                return mouseEventType.overOnZoomControlButton;
            else
                return mouseEventType.overOnBlankSpace;
        }
    }

    class extraButton : controlButton
    {
        protected static Pen extraButtonPen = new Pen(Color.FromArgb(255, 100, 100, 255), 1);
        protected static Brush extraButtonBrush = Brushes.Aquamarine;
        public extraButton(PointF p, Type_ControlButton t)
        {
            p0 = p;
            type = t;
        }

        public override mouseEventType onMouseDown(PointF p)
        {
            if ((Math.Abs(p.X - p0.X) <= r && Math.Abs(p.Y - p0.Y) <= r))
                return mouseEventType.leftClickOnExtraControlButton;
            else
                return mouseEventType.leftClickOnBlankSpace;
        }

        public override mouseEventType onMouseMove(PointF p)
        {
            if ((Math.Abs(p.X - p0.X) <= r && Math.Abs(p.Y - p0.Y) <= r))
                return mouseEventType.overOnExtraControlButton;
            else
                return mouseEventType.overOnBlankSpace;
        }

        public override void draw(ref Graphics g)
        {
            g.FillEllipse(extraButtonBrush, new RectangleF(p0.X - r, p0.Y - r, r << 1, r << 1));
            g.DrawEllipse(extraButtonPen, new RectangleF(p0.X - r, p0.Y - r, r << 1, r << 1));

        }

        public override void setPosition(int x, int y)
        {
            base.setPosition(x, y);
        }
    }



}
