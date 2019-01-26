using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace _2DNormalCalculator
{
    abstract class Utility
    {
        static public LinkedList<System.Drawing.Point> BresenhamLine2D(int x0, int y0, int x1, int y1)
        {
            LinkedList<System.Drawing.Point> points = new LinkedList<System.Drawing.Point>();

           //dx := abs(x1-x0)
            int dx = Math.Abs(x1 - x0);
           //dy := abs(y1-y0) 
            int dy = Math.Abs(y1 - y0);
           //if x0 < x1 then sx := 1 else sx := -1
            int sx;
            if (x0 < x1)
                sx = 1;
            else
                sx = -1;

           //if y0 < y1 then sy := 1 else sy := -1
            int sy;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy;
           //err := dx-dy
         
           //loop
            int e2 = 0;
            while (true)
            {
                //setPixel(x0,y0)
                points.AddFirst(new System.Drawing.Point(x0, y0));
                //if x0 = x1 and y0 = y1 exit loop
                if (x0 == x1 && y0 == y1)
                    break;

                //e2 := 2*err
                e2 = 2 * err;

                //if e2 > -dy then 
                if (e2 > -dy)
                {
                    //err := err - dy
                    err -= dy;
                    //x0 := x0 + sx
                    x0 += sx;
                    //end if
                }

                //if e2 <  dx then 
                if (e2 < dx)
                {
                    //err := err + dx
                    err += dx;
                    //y0 := y0 + sy 
                    y0 += sy;
                    //end if
                }
            }
            //end loop

            return points;
        }

        static public LinkedList<Vector3> BresenhamLine3D(Vector3 p1, Vector3 p2)
        {
            LinkedList<Vector3> points = new LinkedList<Vector3>();

            int x1, y1, z1, x2, y2, z2;
            x1 = (int)p1.X;
            y1 = (int)p1.Y;
            z1 = (int)p1.Z;
            x2 = (int)p2.X;
            y2 = (int)p2.Y;
            z2 = (int)p2.Z;
            
            int i, dx, dy, dz, l, m, n, x_inc, y_inc, z_inc, err_1, err_2, dx2, dy2, dz2;

            int[] pixel = new int[3];

            pixel[0] = x1;
            pixel[1] = y1;
            pixel[2] = z1;

            dx = x2 - x1;
            dy = y2 - y1;
            dz = z2 - z1;
            x_inc = (dx < 0) ? -1 : 1;
            l = Math.Abs(dx);
            y_inc = (dy < 0) ? -1 : 1;
            m = Math.Abs(dy);
            z_inc = (dz < 0) ? -1 : 1;
            n = Math.Abs(dz);
            dx2 = l << 1;
            dy2 = m << 1;
            dz2 = n << 1;

            if ((l >= m) && (l >= n)) 
            {
                err_1 = dy2 - l;
                err_2 = dz2 - l;
                for (i = 0; i < l; i++) 
                {
                    points.AddFirst(new Vector3(pixel[0], pixel[1], pixel[2]));
                    if (err_1 > 0) 
                    {
                        pixel[1] += y_inc;
                        err_1 -= dx2;
                    }
                    if (err_2 > 0) 
                    {
                        pixel[2] += z_inc;
                        err_2 -= dx2;
                    }
                    err_1 += dy2;
                    err_2 += dz2;
                    pixel[0] += x_inc;
                }
            } 
            else if ((m >= l) && (m >= n)) 
            {
                err_1 = dx2 - m;
                err_2 = dz2 - m;
                for (i = 0; i < m; i++) 
                {
                    points.AddFirst(new Vector3(pixel[0], pixel[1], pixel[2]));
                    if (err_1 > 0) {
                        pixel[0] += x_inc;
                        err_1 -= dy2;
                    }
                    if (err_2 > 0) {
                        pixel[2] += z_inc;
                        err_2 -= dy2;
                    }
                    err_1 += dx2;
                    err_2 += dz2;
                    pixel[1] += y_inc;
                }
            }
            else
            {
                err_1 = dy2 - n;
                err_2 = dx2 - n;
                for (i = 0; i < n; i++)
                {
                    points.AddFirst(new Vector3(pixel[0], pixel[1], pixel[2]));
                    if (err_1 > 0)
                    {
                        pixel[1] += y_inc;
                        err_1 -= dz2;
                    }
                    if (err_2 > 0)
                    {
                        pixel[0] += x_inc;
                        err_2 -= dz2;
                    }
                    err_1 += dy2;
                    err_2 += dx2;
                    pixel[2] += z_inc;
                }
            }
            points.AddFirst(new Vector3(pixel[0], pixel[1], pixel[2]));
            return points;
        }

        static public bool IsPixelObscuringLight(int x, int y, int z, Bitmap specialMap)
        {
            Vector3 pixel = new Vector3(specialMap.GetPixel(x, y).R, specialMap.GetPixel(x, y).G, specialMap.GetPixel(x, y).B);
	        float fZ = z;// / 255;
	        return (fZ > pixel.X && fZ < pixel.Y);
        }

        static public bool BresenhamLine3DLgtTest(Vector3 p1, Vector3 p2, Bitmap specialMap)
        {
            int x1, y1, z1, x2, y2, z2;
            x1 = (int)p1.X;
            y1 = (int)p1.Y;
            z1 = (int)p1.Z;
            x2 = (int)p2.X;
            y2 = (int)p2.Y;
            z2 = (int)p2.Z;

            int i, dx, dy, dz, l, m, n, x_inc, y_inc, z_inc, err_1, err_2, dx2, dy2, dz2;

            int px = x1;
            int py = y1;
            int pz = z1;

            dx = x2 - x1;
            dy = y2 - y1;
            dz = z2 - z1;
            x_inc = (dx < 0) ? -1 : 1;
            l = Math.Abs(dx);
            y_inc = (dy < 0) ? -1 : 1;
            m = Math.Abs(dy);
            z_inc = (dz < 0) ? -1 : 1;
            n = Math.Abs(dz);
            dx2 = l << 1;
            dy2 = m << 1;
            dz2 = n << 1;

            if ((l >= m) && (l >= n))
            {
                err_1 = dy2 - l;
                err_2 = dz2 - l;
                for (i = 0; i < l; i++)
                {
                    if (Utility.IsPixelObscuringLight(px, py, pz, specialMap))
                        return true;

                    if (err_1 > 0)
                    {
                        py += y_inc;
                        err_1 -= dx2;
                    }
                    if (err_2 > 0)
                    {
                        pz += z_inc;
                        err_2 -= dx2;
                    }
                    err_1 += dy2;
                    err_2 += dz2;
                    px += x_inc;
                }
            }
            else if ((m >= l) && (m >= n))
            {
                err_1 = dx2 - m;
                err_2 = dz2 - m;
                for (i = 0; i < m; i++)
                {
                    if (Utility.IsPixelObscuringLight(px, py, pz, specialMap))
                        return true;

                    if (err_1 > 0)
                    {
                        px += x_inc;
                        err_1 -= dy2;
                    }
                    if (err_2 > 0)
                    {
                        pz += z_inc;
                        err_2 -= dy2;
                    }
                    err_1 += dx2;
                    err_2 += dz2;
                    py += y_inc;
                }
            }
            else
            {
                err_1 = dy2 - n;
                err_2 = dx2 - n;
                for (i = 0; i < n; i++)
                {
                    if (Utility.IsPixelObscuringLight(px, py, pz, specialMap))
                        return true;

                    if (err_1 > 0)
                    {
                        py += y_inc;
                        err_1 -= dz2;
                    }
                    if (err_2 > 0)
                    {
                        px += x_inc;
                        err_2 -= dz2;
                    }
                    err_1 += dy2;
                    err_2 += dx2;
                    pz += z_inc;
                }
            }
            return false;
        }
    }
}
