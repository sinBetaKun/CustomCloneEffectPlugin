using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustumCloneEffectPlugin
{
    /// <summary>
    /// エフェクトのパラメータ側で大苦戦していて、こちらは全く手を着けていないので、描画時にエラーが発生します。
    /// こちらは自分で何とかします。
    /// </summary>
    internal class CustomCloneEffectProcessor : IVideoEffectProcessor
    {
        readonly CustomCloneEffect item;

        readonly AffineTransform2D transformEffect;

        readonly ID2D1Image? output;

        IGraphicsDevicesAndContext devices;

        ID2D1CommandList? commandList = null;

        ID2D1Image? input;

        bool isFirst = true;

        public ID2D1Image Output => output ?? input ?? throw new NullReferenceException();

        readonly List<AffineTransform2D> transforms;
        readonly List<Opacity> opacityEffects;
        readonly List<ID2D1Image?> outputs;
        private bool[] appear_Array;
        private double[] dstX_Array;
        private double[] dstY_Array;
        private double[] opacity_Array;
        private double[] scale_Array;
        private double[] rotate_Array;
        private bool[] mirror_Array;
        private double[] cntX_Array;
        private double[] cntY_Array;
        private int numOfClones = 0;


        public CustomCloneEffectProcessor(IGraphicsDevicesAndContext devices, CustomCloneEffect item)
        {
            this.item = item;

            this.devices = devices;
            transformEffect = new AffineTransform2D(devices.DeviceContext);//Outputのインスタンスを固定するために、間にエフェクトを挟む
            opacityEffects = new List<Opacity>();
            transforms = new List<AffineTransform2D>();
            outputs = new List<ID2D1Image?>();
            appear_Array = [];
            dstX_Array = [];
            dstY_Array = [];
            opacity_Array = [];
            scale_Array = [];
            rotate_Array = [];
            mirror_Array = [];
            cntX_Array = [];
            cntY_Array = [];
            output = transformEffect.Output;//EffectからgetしたOutputは必ずDisposeする必要がある。Effect側では開放されない。
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            if (updateVectors(frame, length, fps))
            {
                updateOutputs();

                setCommandList();
            }

            isFirst = false;

            return effectDescription.DrawDescription;
        }

        private void setCommandList()
        {
            commandList = devices.DeviceContext.CreateCommandList();
            var dc = devices.DeviceContext;
            dc.Target = commandList;
            dc.BeginDraw();
            dc.Clear(null);

            for (int i = 0; i < numOfClones; i++)
            {
                if (appear_Array[i])
                    if (outputs[i] is ID2D1Image output)
                        dc.DrawImage(output, new Vector2((float)dstX_Array[i], (float)dstY_Array[i]), compositeMode: CompositeMode.SourceOver);
            }

            dc.EndDraw();
            commandList.Close();//CommandListはEndDraw()の後に必ずClose()を呼んで閉じる必要がある
            transformEffect.SetInput(0, commandList, true);
        }

        private void updateOutputs()
        {
            while (transforms.Count < numOfClones)
            {
                transforms.Add(new AffineTransform2D(devices.DeviceContext));
                transforms.Last().SetInput(0, input, true);
            }
            while (transforms.Count > numOfClones)
            {
                transforms.Last().SetInput(0, null, true);
                transforms.Last().Dispose();
                transforms.Remove(transforms.Last());
            }

            for (int i = 0;i < numOfClones; i++)
            {
                transforms[i] = new AffineTransform2D(devices.DeviceContext);
                transforms[i].SetInput(0, input, true);

                var result = (mirror_Array[i])?new Matrix3x2(-1, 0, 0, 1, 2 * (float)cntX_Array[i], 0) :Matrix3x2.Identity;

                float rotate = (float)(rotate_Array[i] % 360 * Math.PI / 180.0);
                if(rotate != 0.0)
                    result *= Matrix3x2.CreateRotation(rotate, new Vector2((float)cntX_Array[i], (float)cntY_Array[i]));

                float scale = (float)(scale_Array[i] / 100.0);
                if(scale != 1.0)
                    result *= Matrix3x2.CreateScale(scale, new Vector2((float)cntX_Array[i], (float)cntY_Array[i]));
                    
                transforms[i].TransformMatrix = result;
            }

            while (opacityEffects.Count < numOfClones)
            {
                opacityEffects.Add(new Opacity(devices.DeviceContext));
                opacityEffects.Last().SetInput(0, null, true);
            }
            while (opacityEffects.Count > numOfClones)
            {
                opacityEffects.Last().SetInput(0, null, true);
                opacityEffects.Last().Dispose();
                opacityEffects.Remove(opacityEffects.Last());
            }

            for(int i = 0; i < numOfClones; i++)
            {
                opacityEffects[i].SetInput(0, transforms[i].Output, true);
                opacityEffects[i].Value = (float)(opacity_Array[i] / 100.0);
            }

            while (outputs.Count > numOfClones)
            {
                if (outputs[numOfClones] is ID2D1Image output) output.Dispose();
                outputs.RemoveAt(numOfClones);
            }
            for (int i = 0; i < numOfClones; i++)
            {
                if (outputs.Count < i + 1) outputs.Add(opacityEffects[i].Output);
                else outputs[i] = opacityEffects[i].Output;
            }
        }

        private bool updateVectors(long frame, long length, int fps)
        {
            bool isOld = false;

            if (numOfClones != item.Clones.Count || isFirst)
            {
                isOld = true;
                numOfClones = item.Clones.Count;
                appear_Array = new bool[numOfClones];
                dstX_Array = new double[numOfClones];
                dstY_Array = new double[numOfClones];
                opacity_Array = new double[numOfClones];
                scale_Array = new double[numOfClones];
                rotate_Array = new double[numOfClones];
                mirror_Array = new bool[numOfClones];
                cntX_Array = new double[numOfClones];
                cntY_Array = new double[numOfClones];
            }
            else
            {
                for (int i = 0; i < numOfClones; i++)
                {
                    var clone = item.Clones[i];
                    var appear = clone.Appear;
                    var dstX = clone.Dst_X.GetValue(frame, length, fps);
                    var dstY = clone.Dst_Y.GetValue(frame, length, fps);
                    var opacity = clone.Opacity.GetValue(frame, length, fps);
                    var scale = clone.Scale.GetValue(frame, length, fps);
                    var rotate = clone.Rotate.GetValue(frame, length, fps);
                    var mirror = (clone.Mirror.GetValue(frame, length, fps) > 0.5);
                    var cntX = clone.Cnt_X.GetValue(frame, length, fps);
                    var cntY = clone.Cnt_Y.GetValue(frame, length, fps);

                    if (appear != appear_Array[i])
                    {
                        isOld = true;
                        break;
                    }

                    if (appear)
                    {
                        if (dstX_Array[i] != dstX || dstY_Array[i] != dstY || opacity_Array[i] != opacity || scale_Array[i] != scale || rotate_Array[i] != rotate || mirror_Array[i] != mirror || cntX_Array[i] != cntX || cntY_Array[i] != cntY)
                        {
                            isOld = true;
                            break;
                        }
                    }
                }
            }

            if (isOld)
            {
                for (int i = 0; i < numOfClones; i++)
                {
                    var clone = item.Clones[i];
                    appear_Array[i] = clone.Appear;
                    if (clone.Appear)
                    {
                        dstX_Array[i] = clone.Dst_X.GetValue(frame, length, fps);
                        dstY_Array[i] = clone.Dst_Y.GetValue(frame, length, fps);
                        opacity_Array[i] = clone.Opacity.GetValue(frame, length, fps);
                        scale_Array[i] = clone.Scale.GetValue(frame, length, fps);
                        rotate_Array[i] = clone.Rotate.GetValue(frame, length, fps);
                        mirror_Array[i] = (clone.Mirror.GetValue(frame, length, fps) > 0.5);
                        cntX_Array[i] = clone.Cnt_X.GetValue(frame, length, fps);
                        cntY_Array[i] = clone.Cnt_Y.GetValue(frame, length, fps);
                    }
                }
            }

            return isOld;
        }

        public void ClearInput()
        {
            input = null;
            transformEffect.SetInput(0, null, true);
            commandList?.Dispose();//前回のUpdateで作成したCommandListを破棄する
            DisposeOutputs(0);
        }

        public void SetInput(ID2D1Image input)
        {
            this.input = input;
            updateOutputs();

            setCommandList();
        }

        public void DisposeOutputs(int count)
        {
            while (opacityEffects.Count > count)
            {
                opacityEffects[count].SetInput(0, null, true);
                opacityEffects[count].Dispose();
                opacityEffects.RemoveAt(count);
            }
            while (transforms.Count > count)
            {
                transforms[count].SetInput(0, null, true);
                transforms[count].Dispose();
                transforms.RemoveAt(count);
            }
            while (outputs.Count > count)
            {
                if(outputs[count] is ID2D1Image output) output.Dispose();
                outputs.RemoveAt(count);
            }
        }

        public void Dispose()
        {
            commandList?.Dispose();//最後のUpdateで作成したCommandListを破棄
            transformEffect.SetInput(0, null, true);//EffectのInputは必ずnullに戻す。
            transformEffect.Dispose();
            DisposeOutputs(0);
            output?.Dispose();//EffectからgetしたOutputは必ずDisposeする必要がある。Effect側では開放されない。
        }
    }
}
