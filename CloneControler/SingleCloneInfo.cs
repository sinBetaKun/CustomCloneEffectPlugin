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

        [Display(GroupName = "単体設定（詳細）", Name = "中心X")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Cnt_X { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "単体設定（詳細）", Name = "中心Y")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Cnt_Y { get; } = new Animation(0, -10000, 10000);


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
            this.Dst_X.CopyFrom(origin.Dst_X);
            this.Dst_Y.CopyFrom(origin.Dst_Y);
            this.Opacity.CopyFrom(origin.Opacity);
            this.Scale.CopyFrom(origin.Scale);
            this.Rotate.CopyFrom(origin.Rotate);
            this.Mirror.CopyFrom(origin.Mirror);
            this.Cnt_X.CopyFrom(origin.Cnt_X);
            this.Cnt_Y.CopyFrom(origin.Cnt_Y);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [Dst_X, Dst_Y, Opacity, Scale, Rotate, Mirror, Cnt_X, Cnt_Y];

    }
}
