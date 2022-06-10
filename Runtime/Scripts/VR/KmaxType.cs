using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
namespace KmaxXR
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KmaxMatrix
    {
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;

        public KmaxMatrix(Matrix4x4 matrix)
        {
            this.m00 = matrix[0, 0];
            this.m01 = matrix[0, 1];
            this.m02 = matrix[0, 2];
            this.m03 = matrix[0, 3];
            this.m10 = matrix[1, 0];
            this.m11 = matrix[1, 1];
            this.m12 = matrix[1, 2];
            this.m13 = matrix[1, 3];
            this.m20 = matrix[2, 0];
            this.m21 = matrix[2, 1];
            this.m22 = matrix[2, 2];
            this.m23 = matrix[2, 3];
            this.m30 = matrix[3, 0];
            this.m31 = matrix[3, 1];
            this.m32 = matrix[3, 2];
            this.m33 = matrix[3, 3];
        }

        public Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 m = Matrix4x4.zero;
            m.m00 = m00;
            m.m01 = m01;
            m.m02 = m02;
            m.m03 = m03;
            m.m10 = m10;
            m.m11 = m11;
            m.m12 = m12;
            m.m13 = m13;
            m.m20 = m20;
            m.m21 = m21;
            m.m22 = m22;
            m.m23 = m23;
            m.m30 = m30;
            m.m31 = m31;
            m.m32 = m32;
            m.m33 = m33;
            return m;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KmaxVector3
    {
        public float x;
        public float y;
        public float z;

        public KmaxVector3(Vector3 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }
      
        public Vector3 ToVector3()
        {
            Vector3 v = Vector3.zero;
            v.x = this.x;
            v.y = this.y;
            v.z = this.z;
            return v;
        }

        public static Vector3 operator *(KmaxVector3 kv,Matrix4x4 m)
        {
            Vector4 v = new Vector4(kv.x, kv.y, kv.z, 1);
            v = m * v;
            return v;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KmaxVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public KmaxVector4(Vector4 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
            w = pos.w;
        }

        public KmaxVector4(Quaternion rot)
        {
            x = rot.x;
            y = rot.y;
            z = rot.z;
            w = rot.w;
        }



        public Vector3 ToVector4()
        {
            Vector4 v = Vector4.zero;
            v.x = this.x;
            v.y = this.y;
            v.z = this.z;
            v.w = this.w;
            return v;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

    }

    public struct KmaxRect
    {
        public KmaxVector3 lt;
        public KmaxVector3 lb;
        public KmaxVector3 rt;
        public KmaxVector3 rb;

        public KmaxRect(KmaxVector3 lt, KmaxVector3 lb, KmaxVector3 rt, KmaxVector3 rb)
        {
            this.lt = lt;
            this.lb = lb;
            this.rt = rt;
            this.rb = rb;
        }
    }

    public enum StereoscopicEye
    {
        main,
        left,
        right,
    }

    public enum ViewArea
    {
        Window,
        Screen,
        DisplayBorder,
    }
}
