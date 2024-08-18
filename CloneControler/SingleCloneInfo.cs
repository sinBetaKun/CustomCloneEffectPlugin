using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;

namespace CustumCloneEffectPlugin.CloneControler
{
    /// <summary>
    /// これのインスタンスは、CustumCloneEffect.cs で CloneOrderList の単位要素になる。
    /// 描画するか否か、と X/Y 座標、不透明度、拡大率、角度、
    /// そして、選択されているか否か(←これがそもそも問題個所かもしれない)
    /// の情報を持たせる。
    /// </summary>
    public class SingleCloneInfo : Animatable
    {
        public bool Appear { get => appear; set { Set(ref appear, value); } }
        bool appear = true;

        [Display(GroupName = "単体設定", Name = "合成モード")]
        [EnumComboBox]
        public BlendModeCCE BlendMode { get => blendMode; set { Set(ref blendMode, value); } }
        BlendModeCCE blendMode = BlendModeCCE.SourceOver;

        [Display(GroupName = "単体設定", Name = "X")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Dst_X { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "単体設定", Name = "Y")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Dst_Y { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "単体設定", Name = "不透明度")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation Opacity { get; } = new Animation(100, 0, 100);

        [Display(GroupName = "単体設定", Name = "拡大率")]
        [AnimationSlider("F1", "%", 0, 200)] 
        public Animation Scale { get; } = new Animation(100, 0, 5000);

        [Display(GroupName = "単体設定", Name = "回転角")]
        [AnimationSlider("F1", "°", -360, 360)]
        public Animation Rotate { get; } = new Animation(0, -36000, 36000);

        [Display(GroupName = "単体設定", Name = "左右反転")]
        [AnimationSlider("F0", "",0,1)]
        public Animation Mirror { get; } = new Animation(0, 0, 1);

        [Display(GroupName = "単体設定（中心位置）", Name = "X")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Cnt_X { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "単体設定（中心位置）", Name = "Y")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Cnt_Y { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "単体設定（中心位置）", Name = "位置を保持")]
        [ToggleSlider]
        public bool KeepPlace { get => keepPlace; set => Set(ref keepPlace, value); }
        bool keepPlace = false;

        [Display(GroupName = "単体設定（サブ拡大率）", Name = "横方向")]
        [AnimationSlider("F1", "%", 0, 200)]
        public Animation Exp_X { get; } = new Animation(100, 0, 5000);

        [Display(GroupName = "単体設定（サブ拡大率）", Name = "縦方向")]
        [AnimationSlider("F1", "%", 0, 200)]
        public Animation Exp_Y { get; } = new Animation(100, 0, 5000);

        [Display(GroupName = "単体設定（クリッピング）", Name = "上")]
        [AnimationSlider("F1", "px", 0, 500)]
        public Animation Top { get; } = new Animation(0, 0, 10000);

        [Display(GroupName = "単体設定（クリッピング）", Name = "下")]
        [AnimationSlider("F1", "px", 0, 500)]
        public Animation Bottom { get; } = new Animation(0, 0, 10000);

        [Display(GroupName = "単体設定（クリッピング）", Name = "左")]
        [AnimationSlider("F1", "px", 0, 500)]
        public Animation Left { get; } = new Animation(0, 0, 10000);

        [Display(GroupName = "単体設定（クリッピング）", Name = "右")]
        [AnimationSlider("F1", "px", 0, 500)]
        public Animation Right { get; } = new Animation(0, 0, 10000);

        [Display(GroupName = "単体設定（ぼかし）", Name = "ぼかし度")]
        [AnimationSlider("F1", "px", 0, 50)]
        public Animation GBlurValue { get; } = new Animation(0, 0, 250);

        [Display(GroupName = "単体設定（方向ブラー）", Name = "ぼかし度")]
        [AnimationSlider("F1", "", 0, 50)]
        public Animation DBlurValue { get; } = new Animation(0, 0, 250);

        [Display(GroupName = "単体設定（方向ブラー）", Name = "角度")]
        [AnimationSlider("F1", "°", -360, 360)]
        public Animation DBlurAngle { get; } = new Animation(0, -36000, 36000);

        [Display(GroupName = "単体設定（他のクローンとの依存関係）", Name = "タグ", Description = "このクローンの名前")]
        [TextEditor(AcceptsReturn = true)]
        public string TagName { get => tagName; set => Set(ref tagName, value); }
        string tagName = string.Empty;

        [Display(GroupName = "単体設定（他のクローンとの依存関係）", Name = "親", Description = "親のクローンの名前")]
        [TextEditor(AcceptsReturn = true)]
        public string Parent { get => parent; set => Set(ref parent, value); }
        string parent = string.Empty;

        [Display(GroupName = "単体設定（他のクローンとの依存関係）", Name = "拡大率依存", Description = "拡大率依存")]
        [ToggleSlider]
        public bool ScaleDependent { get => scaleDependent; set => Set(ref scaleDependent, value); }
        bool scaleDependent = true;

        [Display(GroupName = "単体設定（他のクローンとの依存関係）", Name = "不透明度依存", Description = "不透明度依存")]
        [ToggleSlider]
        public bool OpacityDependent { get => opacityDependent; set => Set(ref opacityDependent, value); }
        bool opacityDependent = true;

        [Display(GroupName = "単体設定（他のクローンとの依存関係）", Name = "回転角依存", Description = "回転角依存")]
        [ToggleSlider]
        public bool RotateDependent { get => rotateDependent; set => Set(ref rotateDependent, value); }
        bool rotateDependent = true;

        public SingleCloneInfo()
        {
            
        }

        /// <summary>
        /// Selectedを除くすべてのパラメータをコピーする。
        /// </summary>
        /// <param name="origin">コピー元</param>
        public SingleCloneInfo(SingleCloneInfo origin)
        {
            this.Appear = origin.Appear;
            this.BlendMode = origin.BlendMode;
            this.Dst_X.CopyFrom(origin.Dst_X);
            this.Dst_Y.CopyFrom(origin.Dst_Y);
            this.Opacity.CopyFrom(origin.Opacity);
            this.Scale.CopyFrom(origin.Scale);
            this.Rotate.CopyFrom(origin.Rotate);
            this.Mirror.CopyFrom(origin.Mirror);
            this.Cnt_X.CopyFrom(origin.Cnt_X);
            this.Cnt_Y.CopyFrom(origin.Cnt_Y);
            this.KeepPlace = origin.KeepPlace;
            this.Exp_X.CopyFrom(origin.Exp_X);
            this.Exp_Y.CopyFrom(origin.Exp_Y);
            this.Top.CopyFrom(origin.Top);
            this.Bottom.CopyFrom(origin.Bottom);
            this.Left.CopyFrom(origin.Left);
            this.Right.CopyFrom(origin.Right);
            this.GBlurValue.CopyFrom(origin.GBlurValue);
            this.DBlurValue.CopyFrom(origin.DBlurValue);
            this.DBlurAngle.CopyFrom(origin.DBlurAngle);
            this.TagName = origin.TagName;
            this.Parent = origin.Parent;
            this.ScaleDependent = origin.ScaleDependent;
            this.OpacityDependent = origin.OpacityDependent;
            this.RotateDependent = origin.RotateDependent;
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [Dst_X, Dst_Y, Opacity, Scale, Rotate, Mirror, Cnt_X, Cnt_Y, Exp_X, Exp_Y, Top, Bottom, Left, Right, GBlurValue, DBlurValue, DBlurAngle];

    }
}
