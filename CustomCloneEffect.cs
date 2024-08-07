using CustumCloneEffectPlugin.CloneControler;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Plugin.Shape;
using YukkuriMovieMaker.Project;

namespace CustumCloneEffectPlugin
{
    [VideoEffect("カスタム複製エフェクト", new[] { "配置" }, new string[] { }, isAviUtlSupported: false)]
    public class CustomCloneEffect : VideoEffectBase
    {
        /// <summary>
        /// エフェクトの名前
        /// </summary>
        public override string Label => "カスタム複製エフェクト";

        [Display(GroupName = "描画順序", Name = "")]
        [CloneOrderChanger(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<SingleCloneInfo> Clones { get => clones; set => Set(ref clones, value); }
        ImmutableList<SingleCloneInfo> clones = [new()];

        /// <summary>
        /// Exoフィルタを作成する。
        /// </summary>
        /// <param name="keyFrameIndex">キーフレーム番号</param>
        /// <param name="exoOutputDescription">exo出力に必要な各種情報</param>
        /// <returns></returns>
        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            //サンプルはSampleD2DVideoEffectを参照
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// 映像エフェクトを作成する
        /// </summary>
        /// <param name="devices">デバイス</param>
        /// <returns>映像エフェクト</returns>
        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new CustomCloneEffectProcessor(devices, this);
        }

        /// <summary>
        /// クラス内のIAnimatableを列挙する。
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IAnimatable> GetAnimatables() => Clones;
    }
}
