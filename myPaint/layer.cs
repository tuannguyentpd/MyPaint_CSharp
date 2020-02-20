using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace myPaint
{


    class layer
    {
        public List<myObject> obArr;
        public Color background=Color.Transparent;
        public int isSelected = -1;

        public layer() {  }

        public layer clone()
        {
            layer answer = new layer();
            answer.background = new Color();
            answer.background = background;
            answer.isSelected = isSelected;
            answer.obArr = new List<myObject>();
            //myObject ob;
            for(int i = 0; i < obArr.Count; i++)
            {
                answer.obArr.Add(obArr[i].clone());
            }
            return answer;

        }

        public layer(Color back)
        {
            obArr = new List<myObject>();
            background = back;
        }

        public void addShape(myObject sh)
        {
            obArr.Add(sh);
            isSelected = obArr.Count() - 1;
        }

        public void clear(Color back)
        {
            obArr.Clear();
            background = back;
            isSelected = -1;
        }

        public void clearSelected()
        {
            isSelected = -1;
        }

        public void deleteSelectedObject()
        {
            if (isSelected != -1)
            {
                obArr.RemoveAt(isSelected);
                isSelected = -1;
            }
        }

        public void save(BinaryWriter f)
        {
            f.Write(background.A);
            f.Write(background.R);
            f.Write(background.G);
            f.Write(background.B);
            f.Write(obArr.Count);
            for(int i = 0; i < obArr.Count; i++)
            {
                f.Write(Convert.ToInt32(obArr[i].type));
                obArr[i].save(f);
            }
        }

        public void load(BinaryReader f)
        {
            int a, r, g, b;
            a = f.ReadByte();
            r = f.ReadByte();
            g = f.ReadByte();
            b = f.ReadByte();
            background = Color.FromArgb(a, r, g, b);
            int count;
            myObjectType type;
            count = f.ReadInt32();
            obArr = new List<myObject>();
            for(int i = 0; i < count; i++)
            {
                type = (myObjectType)f.ReadInt32();
                switch (type) {
                    case myObjectType.LINE:
                        obArr.Add(new myLine());
                        break;
                    case myObjectType.TEXT:
                        obArr.Add(new myText());
                        break;
                    case myObjectType.RECTANGLE:
                        obArr.Add(new myRectangle());
                        break;
                    case myObjectType.PARALLELOGRAM:
                        obArr.Add(new myParallelogram());
                        break;
                    case myObjectType.POLYGON:
                        obArr.Add(new myPolygon());
                        break;
                    case myObjectType.BROKENLINE:
                        obArr.Add(new myBrokenLine());
                        break;
                    case myObjectType.CIRCLE:
                        obArr.Add(new myCircle());
                        break;
                    case myObjectType.CIRCLEARC:
                        obArr.Add(new myCircleArc());
                        break;
                    case myObjectType.ELLIPSE:
                        obArr.Add(new myEllipse());
                        break;
                    case myObjectType.ELLIPSEARC:
                        obArr.Add(new myEllipseArc());
                        break;
                    case myObjectType.BEZIERCURVE:
                        obArr.Add(new myBezierCurve());
                        break;
                    case myObjectType.PARABOLA:
                        obArr.Add(new myParabola());
                        break;
                    case myObjectType.HYPEBOLA:
                        obArr.Add(new myHypebola());
                        break;
                }
                obArr[i].load(f);
            }

        }


        public void draw(ref Graphics g)
        {
            g.Clear(background);
            for (int i = 0; i < obArr.Count; i++)
                obArr[i].draw(ref g);
            if (isSelected > -1)
                obArr[isSelected].drawControlBox(ref g);
        }

        public void drawWithoutSelected(ref Graphics g)
        {
            g.Clear(background);
            for (int i = 0; i < obArr.Count; i++)
                if(i!=isSelected)
                    obArr[i].draw(ref g);
        }

        public mouseEventType onMouseDown(Point p)
        {
            //isSelected = -1;
            mouseEventType temp = mouseEventType.leftClickOnBlankSpace;

            //if (isSelected != -1)
            //{
            //    temp = obArr[isSelected].onMouseDown(p,true);
            //    if (temp == mouseEventType.leftClickOnNotSelectedObject||temp==mouseEventType.leftClickInControlBox)
            //        return mouseEventType.leftClickOnSelectedObject;
            //    else if (temp != mouseEventType.leftClickOnBlankSpace)
            //        return temp;
            //}
            for (int i = obArr.Count - 1; i >= 0; i--)
            {
                temp = obArr[i].onMouseDown(p, i==isSelected);
                if (temp == mouseEventType.leftClickOnNotSelectedObject)
                {
                    isSelected = i;
                    return temp;
                }
                else if (temp != mouseEventType.leftClickOnBlankSpace)
                    return temp;
            }
            isSelected = -1;
            return mouseEventType.leftClickOnBlankSpace;
        }

        public myObject getSelectedObject()
        {
            if (isSelected != -1)
                return obArr[isSelected];
            else
                return new myRectangle();
        }


        

        public mouseEventType onMouseMove(Point p)
        {
            mouseEventType temp = mouseEventType.leftClickOnBlankSpace;
            for (int i = obArr.Count - 1; i >= 0; i--)
            {
                temp = obArr[i].onMouseMove(p,i==isSelected);
                if (temp != mouseEventType.overOnBlankSpace)
                {
                    return temp;
                }
            }
            return mouseEventType.overOnBlankSpace;
        }

        public void onMouseUp(Point startPoint, Point previousPoint, Point endPoint, mouseEventType down,myObject sh)
        {
            switch (down) {
                case mouseEventType.leftClickOnBlankSpace:
                    if (Math.Abs(startPoint.X - endPoint.X) > 3 && Math.Abs(startPoint.Y - endPoint.Y) > 3)
                    {
                        sh.adjust(startPoint, endPoint, false);
                        addShape(sh);
                    }
                    break;
                case mouseEventType.leftClickInControlBox:
                case mouseEventType.leftClickOnSelectedObject:
                case mouseEventType.leftClickOnNotSelectedObject:
                    sh.translate(previousPoint, endPoint);
                    obArr[isSelected]=sh;
                    break;
                case mouseEventType.leftClickOnRotateControlButton:
                    sh.rotate(previousPoint, endPoint);
                    obArr[isSelected]=sh;
                    break; ;
                case mouseEventType.leftClickOnZoomControlButton:
                    sh.scale(previousPoint, endPoint);
                    obArr[isSelected]=sh;
                    break;
                case mouseEventType.leftClickOnExtraControlButton:
                    sh.moveExtraPoint(previousPoint, endPoint);
                    obArr[isSelected] = sh;
                    break;
            }

        }
    }
}
