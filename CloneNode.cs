using CustumCloneEffectPlugin;
using CustumCloneEffectPlugin.CloneControler;
using System.Collections.Immutable;
using System.Numerics;
using Vortice.Direct2D1.Effects;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using System.Windows.Media.Effects;
using Windows.Win32.UI.KeyboardAndMouseInput;
using System.Windows;
using System.Xml.Linq;

namespace CustomCloneEffectPlugin
{
    public class CloneNode : IDisposable
    {
        readonly Crop cropEffect;
        readonly AffineTransform2D transform;
        readonly Opacity opacityEffect;
        readonly GaussianBlur gblurEffect;
        readonly DirectionalBlur dblurEffect;

        IGraphicsDevicesAndContext devices;
        public ID2D1Image? Output;

        public bool Appear { get; set; }
        public BlendModeCCE BlendMode { get; set; }
        public double Dst_X { get; set; }
        public double Dst_Y { get; set; }
        public double Opacity { get; set; }
        public double Scale { get; set; }
        public double Rotate { get; set; }
        public bool Mirror { get; set; }
        public double Cnt_X { get; set; }
        public double Cnt_Y { get; set; }
        public bool KeepPlace { get; set; }
        public double Exp_X { get; set; }
        public double Exp_Y { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double GBlurValue { get; set; }
        public double DBlurValue { get; set; }
        public double DBlurAngle { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;
        public bool ScaleDependent { get; set; }
        public bool OpacityDependent { get; set; }
        public bool RotateDependent { get; set; }

        public Vector2 Shift { get; set; }
        public float Opacity2 { get; set; }
        public float Scale2 { get; set; }
        public float Rotate2 { get; set; }

        public ImmutableList<CloneNode> ParentPath { get; set; } = [];

        public CloneNode(IGraphicsDevicesAndContext devices)
        {
            this.devices = devices;
            cropEffect = new Crop(devices.DeviceContext);
            transform = new AffineTransform2D(devices.DeviceContext);
            opacityEffect = new Opacity(devices.DeviceContext);
            gblurEffect = new GaussianBlur(devices.DeviceContext);
            dblurEffect = new DirectionalBlur(devices.DeviceContext);
        }

        public CloneNode(CloneNode origin)
        {
            devices = origin.devices;
            cropEffect = new Crop(devices.DeviceContext);
            transform = new AffineTransform2D(devices.DeviceContext);
            opacityEffect = new Opacity(devices.DeviceContext);
            gblurEffect = new GaussianBlur(devices.DeviceContext);
            dblurEffect = new DirectionalBlur(devices.DeviceContext);
            Appear = origin.Appear;
            BlendMode = origin.BlendMode;
            Dst_X = origin.Dst_X;
            Dst_Y = origin.Dst_Y;
            Opacity = origin.Opacity;
            Scale = origin.Scale;
            Rotate = origin.Rotate;
            Mirror = origin.Mirror;
            Cnt_X = origin.Cnt_X;
            Cnt_Y = origin.Cnt_Y;
            KeepPlace = origin.KeepPlace;
            Exp_X = origin.Exp_X;
            Exp_Y = origin.Exp_Y;
            Top = origin.Top;
            Bottom = origin.Bottom;
            Left = origin.Left;
            Right = origin.Right;
            GBlurValue = origin.GBlurValue;
            DBlurValue = origin.DBlurValue;
            DBlurAngle = origin.DBlurAngle;
            TagName = origin.TagName;
            Parent = origin.Parent;
            ScaleDependent = origin.ScaleDependent;
            OpacityDependent = origin.OpacityDependent;
            RotateDependent = origin.RotateDependent;
            ParentPath = [.. origin.ParentPath];
        }

        public CloneNode(IGraphicsDevicesAndContext devices , SingleCloneInfo clone, long length, long frame, int fps)
        {
            this.devices = devices;
            cropEffect = new Crop(devices.DeviceContext);
            transform = new AffineTransform2D(devices.DeviceContext);
            opacityEffect = new Opacity(devices.DeviceContext);
            gblurEffect = new GaussianBlur(devices.DeviceContext);
            dblurEffect = new DirectionalBlur(devices.DeviceContext);
            Appear = clone.Appear;
            BlendMode = clone.BlendMode;
            Dst_X = clone.Dst_X.GetValue(frame, length, fps);
            Dst_Y = clone.Dst_Y.GetValue(frame, length, fps);
            Opacity = clone.Opacity.GetValue(frame, length, fps);
            Scale = clone.Scale.GetValue(frame, length, fps);
            Rotate = clone.Rotate.GetValue(frame, length, fps);
            Mirror = (clone.Mirror.GetValue(frame, length, fps) > 0.5);
            Cnt_X = clone.Cnt_X.GetValue(frame, length, fps);
            Cnt_Y = clone.Cnt_Y.GetValue(frame, length, fps);
            KeepPlace = clone.KeepPlace;
            Exp_X = clone.Exp_X.GetValue(frame, length, fps);
            Exp_Y = clone.Exp_Y.GetValue(frame, length, fps);
            Top = clone.Top.GetValue(frame, length, fps);
            Bottom = clone.Bottom.GetValue(frame, length, fps);
            Left = clone.Left.GetValue(frame, length, fps);
            Right = clone.Right.GetValue(frame, length, fps);
            GBlurValue = clone.GBlurValue.GetValue(frame, length, fps);
            DBlurValue = clone.DBlurValue.GetValue(frame, length, fps);
            DBlurAngle = clone.DBlurAngle.GetValue(frame, length, fps);
            TagName = clone.TagName;
            Parent = clone.Parent;
            ScaleDependent = clone.ScaleDependent;
            OpacityDependent = clone.OpacityDependent;
            RotateDependent = clone.RotateDependent;
        }

        public bool Update(SingleCloneInfo clone, long length, long frame, int fps)
        {
            var appear = clone.Appear;
            var blendMode = clone.BlendMode;
            var dstX = clone.Dst_X.GetValue(frame, length, fps);
            var dstY = clone.Dst_Y.GetValue(frame, length, fps);
            var opacity = clone.Opacity.GetValue(frame, length, fps);
            var scale = clone.Scale.GetValue(frame, length, fps);
            var rotate = clone.Rotate.GetValue(frame, length, fps);
            var mirror = (clone.Mirror.GetValue(frame, length, fps) > 0.5);
            var cntX = clone.Cnt_X.GetValue(frame, length, fps);
            var cntY = clone.Cnt_Y.GetValue(frame, length, fps);
            var keepPlace = clone.KeepPlace;
            var expX = clone.Exp_X.GetValue(frame, length, fps);
            var expY = clone.Exp_Y.GetValue(frame, length, fps);
            var top = clone.Top.GetValue(frame, length, fps);
            var bottom = clone.Bottom.GetValue(frame, length, fps);
            var left = clone.Left.GetValue(frame, length, fps);
            var right = clone.Right.GetValue(frame, length, fps);
            var gBlurValue = clone.GBlurValue.GetValue(frame, length, fps);
            var dBlurValue = clone.DBlurValue.GetValue(frame, length, fps);
            var dBlurAngle = clone.DBlurAngle.GetValue(frame, length, fps);
            var tagName = clone.TagName;
            var parent = clone.Parent;
            var scaleDependent = clone.ScaleDependent;
            var opacityDependent = clone.OpacityDependent;
            var rotateDependent = clone.RotateDependent;

            bool isOld = false;

            if (appear != Appear || BlendMode != blendMode || Dst_X != dstX || Dst_Y != dstY || Opacity != opacity || Scale != scale || Rotate != rotate || Mirror != mirror || Cnt_X != cntX || Cnt_Y != cntY || keepPlace != KeepPlace || Exp_X != expX || Exp_Y != expY || Top != top || Bottom != bottom || Left != left || Right != right || GBlurValue != gBlurValue || DBlurValue != dBlurValue || DBlurAngle != dBlurAngle || TagName != tagName || Parent != parent || ScaleDependent != scaleDependent || OpacityDependent != opacityDependent || RotateDependent != rotateDependent)
            {
                isOld = true;
                Appear = appear;
                BlendMode = blendMode;
                Dst_X = dstX;
                Dst_Y = dstY;
                Opacity = opacity;
                Scale = scale;
                Rotate = rotate;
                Mirror = mirror;
                Cnt_X = cntX;
                Cnt_Y = cntY;
                KeepPlace = keepPlace;
                Exp_X = expX;
                Exp_Y = expY;
                Top = top;
                Bottom = bottom;
                Left = left;
                Right = right;
                GBlurValue = gBlurValue;
                DBlurValue = dBlurValue;
                DBlurAngle = dBlurAngle;
                TagName = tagName;
                Parent = parent;
                ScaleDependent = scaleDependent;
                OpacityDependent = opacityDependent;
                RotateDependent = rotateDependent;
            }


            return isOld;
        }

        public void CommitOutput(ID2D1Image? input)
        {
            ID2D1Image? output = input;

            // リセット
            dblurEffect.SetInput(0, null, true);
            gblurEffect.SetInput(0, null, true);
            opacityEffect.SetInput(0, null, true);
            transform.SetInput(0, null, true);
            cropEffect.SetInput(0, null, true);

            // 依存関係から最終的な描画位置などを計算する。
            double x = 0, y = 0, scale = 1, opacity = 1, rotate = 0;
            foreach (var node in ParentPath)
            {
                double x2 = node.Dst_X + (node.KeepPlace ? node.Cnt_X:0), y2 = node.Dst_Y + (node.KeepPlace ? node.Cnt_Y : 0);
                float rotate2 = (float)(rotate * Math.PI / 180);
                x += scale * (x2 * Math.Cos(rotate2) - y2 * Math.Sin(rotate2));
                y += scale * (x2 * Math.Sin(rotate2) + y2 * Math.Cos(rotate2));
                opacity = ((node.OpacityDependent) ? opacity : 1) * node.Opacity / 100;
                scale = ((node.ScaleDependent) ? scale : 1) * node.Scale / 100;
                rotate = ((node.RotateDependent) ? rotate : 0) + node.Rotate;
            }
            float rotate3 = (float)(rotate * Math.PI / 180);
            double x3 = Dst_X - (KeepPlace ? 0 : Cnt_X), y3 = Dst_Y - (KeepPlace ? 0 : Cnt_Y);
            x += scale * (x3 * Math.Cos(rotate3) - y3 * Math.Sin(rotate3));
            y += scale * (x3 * Math.Sin(rotate3) + y3 * Math.Cos(rotate3));
            opacity = (OpacityDependent ? opacity : 1) * Opacity / 100;
            scale = (ScaleDependent ? scale : 1) * Scale / 100;
            Shift = new Vector2((float)x, (float)y);
            Opacity2 = (float)opacity;
            Scale2 = (float)scale;
            Rotate2 = (float)(RotateDependent ? (rotate * Math.PI / 180) : 0);

            // 四角形切り抜きエフェクト
            if (Left != 0.0 || Top != 0.0 || Right != 0.0 || Bottom != 0.0)
            {
                var defRect = devices.DeviceContext.GetImageLocalBounds(output);
                var defVect = new Vector4(defRect.Left, defRect.Top, defRect.Right, defRect.Bottom);
                var X = (float)(defVect.X + Left);
                var Y = (float)(defVect.Y + Top);
                var Z = (float)(defVect.Z - Right);
                var W = (float)(defVect.W - Bottom);
                cropEffect.Rectangle = new Vector4(X, Y, Z, W);
                cropEffect.SetInput(0, output, true);
                output = cropEffect.Output;
            }

            // アフィン変換(インスタンスOutputを固定するために，このエフェクトは無条件に間に挟む)
            var result = (Mirror) ? new Matrix3x2(-1, 0, 0, 1, 2 * (float)Cnt_X, 0) : Matrix3x2.Identity;
            float expX = (float)(Exp_X / 100.0);
            float expY = (float)(Exp_Y / 100.0);
            float rotate4 = (float)(Rotate * Math.PI / 180);
            float cx = (float)(Cnt_X * Math.Cos(Rotate2) - Cnt_Y * Math.Sin(Rotate2));
            float cy = (float)(Cnt_X * Math.Sin(Rotate2) + Cnt_Y * Math.Cos(Rotate2));
            if (Scale2 != 1.0 || expX != 1.0 || expY != 1.0)
                result *= Matrix3x2.CreateScale(Scale2 * expX, Scale2 * expY, new Vector2(cx, cy));
            if (Rotate2 != 0.0)
                result *= Matrix3x2.CreateRotation(Rotate2);
            if (Rotate != 0.0)
                result *= Matrix3x2.CreateRotation((float)rotate4, new Vector2(cx, cy));
            //MessageBox.Show($"TagName={TagName}\nRotate2={Rotate2}");
            transform.TransformMatrix = result;
            transform.SetInput(0, output, true);
            output = transform.Output;

            // 不透明エフェクト
            if (Opacity2 != 1.0)
            {
                opacityEffect.Value = Opacity2;
                opacityEffect.SetInput(0, output, true);
                output = opacityEffect.Output;
            }

            // ぼかしエフェクト
            if (GBlurValue != 0.0)
            {
                gblurEffect.StandardDeviation = (float)GBlurValue;
                gblurEffect.SetInput(0, output, true);
                output = gblurEffect.Output;
            }

            // 方向ブラーエフェクト
            if (DBlurValue != 0.0)
            {
                dblurEffect.StandardDeviation = (float)DBlurValue;
                dblurEffect.Angle = (float)(- DBlurAngle - rotate);
                dblurEffect.SetInput(0, output, true);
                output = dblurEffect.Output;
            }

            // 出力イメージ
            Output = output;
        }

        public void Dispose()
        {
            dblurEffect.SetInput(0, null, true);
            dblurEffect.Dispose();

            gblurEffect.SetInput(0, null, true);
            gblurEffect.Dispose();

            opacityEffect.SetInput(0, null, true);
            opacityEffect.Dispose();

            transform.SetInput(0, null, true);
            transform.Dispose();

            cropEffect.SetInput(0, null, true);
            cropEffect.Dispose();

            Output?.Dispose();
        }
    }
}
