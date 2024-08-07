# CustomCloneEffectPlugin

![CustomCloneEffectDescription 2024-08-07 15-48-20](https://github.com/user-attachments/assets/cacdb391-c325-4d2a-b282-442da2dca239)
### 概要
　これは『ゆっくりMovieMaker4』のプラグインです。映像やテキストなどの視覚的なものを，複数の位置に複製できるエフェクトを提供します。調節可能なパラメータは描画位置だけでなく，不透明度，拡大率，回転角，左右反転，中心位置も指定できます。「位置や大きさは違えど，同じ視覚メディアを複数個描画するためだけにたくさんのレイヤーを使いたくない」というような場面によく遭遇するなら，このプラグインがあなたの悩みを解決してくれるでしょう。

### 分類
　映像エフェクトとして使うことができ，『配置』カテゴリに分類されます。

### 導入時の注意
　このプラグインをインストールする前に，お使いの『ゆっくりMovieMaker4』のバージョンが`4.31.0.0`以降であることを確認してください。もしこの条件を満たさない場合，プラグインは正常な動作をしませんので，ご注意ください。

### 導入方法
1. Releasesからご希望のバージョンを選択し，`CustomCloneEffectPlugin.ymme`をダウンロードしてください。
2. ダウンロードした`ymme`ファイルが`ゆっくりMovieMaker4 プラグインファイル (.ymme)`として認識されていることを`Alt`+`Enter`で確認して下さい。
3. ダウンロードした`ymme`ファイルをダブルクリックで開くと「プラグインのインストール」というウィンドウが表示されます。
4. 「このプラグインについて」の内容と「プラグインのインストールに関する注意事項」の内容を確認してください。
5. 「このプラグインの開発者を信頼する」にチェックを入れて「インストール」を押してください。
6. 再度『ゆっくりMovieMaker4』を起動し，視覚アイテム[^1]の「映像エフェクト」に「カスタム複製エフェクト」が追加できるようになれば，導入完了です。

[^1]:ここではテキストアイテム，動画アイテム，画像アイテム，図形アイテムなどの，アイテム編集ウィンドウに「映像エフェクト」が含まれるアイテムを指しています。

### 使い方
* 描画順序
  * 描画されるクローン[^2]が要素としてこのリストに並びます。以後この要素をクローンブロックと呼びます。
  * クローンブロックについているチェックボックスにチェックが入っているときのみ，対応するクローンが描画されます。
  * クローンブロックについている黒い四角形の中に書かれた数値は，最初のフレームでクローンが描画される座標を表しています。
  * 選択しているクローンブロックに対応するクローンのパラメータを，リストの下にあるコントローラーで調節できます。
  * `+`ボタンで，選択されているクローンブロックの１つ下に新しくクローンブロックが追加されます。
  * `-`ボタンで，選択されているクローンブロックが消去されます。
  * `複`ボタンで，選択されているクローンブロックが１つ下に複製されます。
  * `▲`ボタンで，選択されているクローンブロックが１つ上のクローンブロックと入れ替わります。
  * `▼`ボタンで，選択されているクローンブロックが１つ下のクローンブロックと入れ替わります。

  ![image](https://github.com/user-attachments/assets/4c1ffbf8-92f7-47ae-b8f2-39ff028cbd96)

[^2]:ここでは，本エフェクトによって描画された複製物のことを指します。

* 単体設定
  * `X`と`Y`で，クローンの描画位置を指定します。
  * `不透明度`で，クローンの不透明度を指定します。
  * `拡大率`で，クローンの拡大率を指定します。
  * `回転角`で，クローンを時計回りに回転させる角度を指定します。
  * `左右反転`の値が`1`になっているときは左右反転します。[^3]

  ![image](https://github.com/user-attachments/assets/c6b44f23-6431-4b4e-9682-f2fa77c7f4c3)

[^3]:実際にユーザーが入力できる値は`0`と`1`のみですが，内部的には`0.5`未満を`0`，そうでない時を`1`とみなしています。

* 単体設定（詳細）
  * `中心X`と`中心Y`で，クローンの拡大，回転の中心を指定します。

  ![image](https://github.com/user-attachments/assets/42050f71-86d9-4f78-a4c0-90e7d516d7ef)

### 作ったきっかけ
　現在，立ち絵プラグインの構想を練っているのですが，その仕様の一つに「パーツごとに異なる動きを調節する」というものがあり，当然そのような操作ができるコントローラーを実装する必要があり，その機構の制作に手を付けてから５ヶ月が過ぎました。しかし，全体を完成させてからではいくつかの機構を調整していくと訳が分からなくなってしまいます。そのことは最初から分かってたものの，どのような形で「パーツごとに異なる動きを調節する」コントローラーを単体で実装するかが決まらなかったんです。

 そして１ヶ月前，唐突にこのプラグインを思いつき，作ろうとしたものの思うようにいかず，我らが神主 饅頭遣いさんに相談し，時間はかかったものの何とか完成までもってくることができました。饅頭遣いさんにはとても感謝しています。そして，前回の『時間差複製エフェクト』の時みたいな実装ミスを残さないよう，@kotolin-sub さんに手伝ってもらいました。お二人さん，ありがとうございます！

### 不具合，問題，要望がある場合
　[X(旧Twitter)にてDMください](https://twitter.com/sinBetaKun)。また，Discordで私と何らかのつながりがあるなら，DiscordでもDMしちゃって構いません。**ただし私が不快に感じることをDMで繰り返すようであれば容赦なくブロックしますのでご注意ください。**

---

### sinβ のDiscordサーバー『<| WAVER |>』
参加する場合はルールはしっかり守りましょう。メンバー認証には半日以上待つ場合がほとんどですので，承知の上で申請してください。

https://discord.gg/pCtyFvjEcj
