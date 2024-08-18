using CustomCloneEffectPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vortice;
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
        IGraphicsDevicesAndContext devices;
        ID2D1CommandList? commandList = null;
        ID2D1Image? input;
        bool isFirst = true;

        public ID2D1Image Output { get; }
        /*
        readonly List<Crop> cropEffects;
        readonly List<AffineTransform2D> transforms;
        readonly List<Opacity> opacityEffects;
        readonly List<GaussianBlur> blurEffects;
        readonly List<ID2D1Image?> outputs;
         */
        private List<CloneNode> CloneNodes = [];
        private int numOfClones = 0;

        RawRectF inputRect;


        public CustomCloneEffectProcessor(IGraphicsDevicesAndContext devices, CustomCloneEffect item)
        {
            this.item = item;

            this.devices = devices;
            /*
            cropEffects = new List<Crop>();
            transformEffect = new AffineTransform2D(devices.DeviceContext);//Outputのインスタンスを固定するために、間にエフェクトを挟む
            opacityEffects = new List<Opacity>();
            blurEffects = new List<GaussianBlur>();
            */
            transformEffect = new AffineTransform2D(devices.DeviceContext);//Outputのインスタンスを固定するために、間にエフェクトを挟む
            Output = transformEffect.Output;//EffectからgetしたOutputは必ずDisposeする必要がある。Effect側では開放されない。
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            if (updateClonesValues(frame, length, fps))
            {
                commandList?.Dispose();

                UpdateParentPaths();

                updateOutputs();

                setCommandList();
            }

            isFirst = false;

            return effectDescription.DrawDescription;
        }

        private void UpdateParentPaths()
        {
            List<CloneNode> independent = [];
            List<CloneNode> parents = [];
            List<CloneNode> children = [];
            int numOfChildren = 0;
            
            foreach(var cloneNode in CloneNodes)
            {
                cloneNode.ParentPath = [];
                if (cloneNode.Parent == string.Empty)
                {
                    if (cloneNode.TagName == string.Empty || cloneNode.TagName == cloneNode.Parent) independent.Add(cloneNode);
                    else parents.Add(cloneNode);
                }
                else
                {
                    children.Add(cloneNode);
                    numOfChildren++;
                }
            }

            while(numOfChildren > 0)
            {

                for(int i = 0;i < children.Count;)
                {
                    var matched = from x in parents 
                                  where (x.TagName == children[i].Parent) select x;

                    if (matched.Count() > 0)
                    {
                        children[i].ParentPath = matched.First().ParentPath.Add(matched.First());
                        parents.Add(children[i]);
                        children.RemoveAt(i);
                    }
                    else i++;
                }

                if (numOfChildren == children.Count) break;
                numOfChildren = children.Count;
            }
            /*
            foreach(var cloneNode in CloneNodes)
            {
                double x = 0, y = 0, scale = 1, opacity = 1, rotate = 0;
                var path = cloneNode.ParentPath.Add(cloneNode);
                foreach (var node in path)
                {
                    double x2 = node.Dst_X + node.Cnt_X, y2 = node.Dst_Y + node.Cnt_Y;
                    float rotate2 = (float)((rotate % 360) * Math.PI / 180);
                    x += (float)(scale * (x2 * Math.Cos(rotate2) - y2 * Math.Sin(rotate2)));
                    y += (float)(scale * (x2 * Math.Sin(rotate2) + y2 * Math.Cos(rotate2)));
                    opacity = ((node.OpacityDependent) ? opacity : 1) * node.Opacity / 100;
                    scale = ((node.ScaleDependent) ? scale : 1) * node.Scale / 100;
                    rotate = ((node.RotateDependent) ? rotate : 0) + node.Rotate;
                }
                cloneNode.Shift = new Vector2 ((float)x, (float)y);
                cloneNode.Opacity2 = opacity;
                cloneNode.Scale2 = scale;
                cloneNode.Rotate2 = (rotate % 360) * Math.PI / 180;
            }
             */
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
                if (CloneNodes[i].Appear)
                {
                    if (CloneNodes[i].Output is ID2D1Image output)
                    {
                        var vec2 = CloneNodes[i].Shift;
                        switch (CloneNodes[i].BlendMode)
                        {
                            case BlendModeCCE.SourceOver:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.SourceOver);
                                break;

                            case BlendModeCCE.Plus:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.Plus);
                                break;

                            case BlendModeCCE.DestinationOver:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.DestinationOver);
                                break;

                            case BlendModeCCE.DestinationOut:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.DestinationOut);
                                break;

                            case BlendModeCCE.SourceAtop:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.SourceAtop);
                                break;

                            case BlendModeCCE.XOR:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.Xor);
                                break;

                            case BlendModeCCE.MaskInverseErt:
                                dc.DrawImage(output, vec2, compositeMode: CompositeMode.MaskInverseErt);
                                break;

                            case BlendModeCCE.Multiply:
                                dc.BlendImage(output, BlendMode.Multiply, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Screen:
                                dc.BlendImage(output, BlendMode.Screen, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Darken:
                                dc.BlendImage(output, BlendMode.Darken, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Lighten:
                                dc.BlendImage(output, BlendMode.Lighten, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Dissolve:
                                dc.BlendImage(output, BlendMode.Dissolve, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.ColorBurn:
                                dc.BlendImage(output, BlendMode.ColorBurn, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.LinearBurn:
                                dc.BlendImage(output, BlendMode.LinearBurn, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.DarkerColor:
                                dc.BlendImage(output, BlendMode.DarkerColor, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.LighterColor:
                                dc.BlendImage(output, BlendMode.LighterColor, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.ColorDodge:
                                dc.BlendImage(output, BlendMode.ColorDodge, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.LinearDodge:
                                dc.BlendImage(output, BlendMode.LinearDodge, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Overlay:
                                dc.BlendImage(output, BlendMode.Overlay, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.SoftLight:
                                dc.BlendImage(output, BlendMode.SoftLight, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.HardLight:
                                dc.BlendImage(output, BlendMode.HardLight, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.VividLight:
                                dc.BlendImage(output, BlendMode.VividLight, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.LinearLight:
                                dc.BlendImage(output, BlendMode.LinearLight, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.PinLight:
                                dc.BlendImage(output, BlendMode.PinLight, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.HardMix:
                                dc.BlendImage(output, BlendMode.HardMix, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Difference:
                                dc.BlendImage(output, BlendMode.Difference, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Exclusion:
                                dc.BlendImage(output, BlendMode.Exclusion, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Hue:
                                dc.BlendImage(output, BlendMode.Hue, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Saturation:
                                dc.BlendImage(output, BlendMode.Saturation, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Color:
                                dc.BlendImage(output, BlendMode.Color, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Luminosity:
                                dc.BlendImage(output, BlendMode.Luminosity, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Subtract:
                                dc.BlendImage(output, BlendMode.Subtract, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendModeCCE.Division:
                                dc.BlendImage(output, BlendMode.Division, vec2, null, InterpolationMode.MultiSampleLinear);
                                break;
                        }

                    }
                }
            }

            dc.EndDraw();
            commandList.Close();//CommandListはEndDraw()の後に必ずClose()を呼んで閉じる必要がある
            transformEffect.SetInput(0, commandList, true);
        }

        private void updateOutputs()
        {
            foreach (var cloneNode in CloneNodes)
                cloneNode.CommitOutput(input);

        }

        private bool updateClonesValues(long frame, long length, int fps)
        {
            bool isOld = false;

            if (numOfClones != item.Clones.Count || isFirst)
            {
                isOld = true;
                numOfClones = item.Clones.Count;
                RemoveNodes(0);
                for (int i = 0; i < numOfClones; i++)
                    CloneNodes.Add(new CloneNode(devices, item.Clones[i], length, frame, fps));
            }
            else
            {
                for (int i = 0; i < numOfClones; i++)
                {
                    if(CloneNodes[i].Update(item.Clones[i], length, frame, fps))
                    {
                        isOld = true;
                    }
                }
                var defRect = devices.DeviceContext.GetImageLocalBounds(input);
                if (!defRect.Equals(inputRect))
                    isOld = true;

                inputRect = defRect;
            }
            return isOld;
        }

        public void ClearInput()
        {
            input = null;
            transformEffect.SetInput(0, null, true);
            commandList?.Dispose();//前回のUpdateで作成したCommandListを破棄する
            RemoveNodes(0);
        }

        public void SetInput(ID2D1Image input)
        {
            commandList?.Dispose();

            this.input = input;

            numOfClones = 0;

            UpdateParentPaths();

            updateOutputs();

            setCommandList();
        }

        public void RemoveNodes(int count)
        {
            while (CloneNodes.Count > count)
            {
                CloneNodes[count].Dispose();
                CloneNodes.RemoveAt(count);
            }
        }

        public void Dispose()
        {
            commandList?.Dispose();//最後のUpdateで作成したCommandListを破棄
            transformEffect.SetInput(0, null, true);//EffectのInputは必ずnullに戻す。
            transformEffect.Dispose();
            RemoveNodes(0);
            Output.Dispose();//EffectからgetしたOutputは必ずDisposeする必要がある。Effect側では開放されない。
        }
    }
}
