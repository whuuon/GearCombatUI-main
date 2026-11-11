#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Native, editable reconstruction of the two Level 12 layouts in the assignment.
// Artwork is exported from the original Figma layers; controls, text and gear stay separate.
public static class AssignmentUIBuilder
{
    const string Art = "Assets/GearFinal/Assets/";
    const string Ref = "Assets/CombatUI/FigmaSprites/";
    static TMP_FontAsset font;
    static readonly Color Ink = new Color32(44, 58, 83, 255);

    [MenuItem("Combat UI/Match Assignment Reference")]
    public static void Build() => CombatUIDemoBuilder.Build();

    public static void Apply(UIPreviewSwitcher preview)
    {
        foreach (string p in AssetDatabase.FindAssets("t:Texture2D", new[] {Ref.TrimEnd('/')}))
        {
            var path = AssetDatabase.GUIDToAssetPath(p);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 4096;
            ti.npotScale = TextureImporterNPOTScale.None;
            var settings=new TextureImporterSettings();
            ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
            ti.filterMode = FilterMode.Bilinear;
            if (Path.GetFileName(path) == "BoardPanel.png") ti.spriteBorder = new Vector4(80,80,80,80);
            ti.SaveAndReimport();
        }
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Art+"Font/Lalezar-Regular SDF.asset");
        for (int i = 0; i < preview.states.Length; i++)
        {
            var st = preview.states[i];
            st.worldUI.SetActive(false);
            st.background.SetActive(false);
            // The reference backdrop below replaces the Stage 5 world snapshot.
            st.worldUI = null;
            st.background = null;
            BuildHUD(st.hud, i == 1);
        }
        preview.ShowState(0);
        preview.ShowPopup(-1);
    }

    static void BuildHUD(GameObject hud, bool battle)
    {
        foreach (Transform c in hud.transform)
            if (c.name != "Popups") { c.gameObject.SetActive(false); c.name = "Snapshot_" + c.name; }
        var scaler = hud.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(740, 1616);
        scaler.matchWidthOrHeight = 1;
        var responsive = hud.GetComponent<ResponsiveCanvasScaler>();
        responsive.minWidth = 740;
        var ui = Rect("ReferenceUI", hud.transform);
        Stretch(ui);
        var layout = hud.AddComponent<AssignmentResponsiveLayout>();
        layout.battle = battle;
        layout.sky = Img("Environment",ui,Ref+"Environment.png",0,0,1616,1616).rectTransform;
        layout.field = Rect("Battlefield",ui);
        if (battle) World(layout.field);
        layout.header = Rect("LevelProgressBar", ui);
        Header(layout.header, battle);
        layout.board = Rect("GearContainer", ui);
        Board(layout.board, battle);
        layout.shop = Rect("ShopContainer", ui);
        Shop(layout.shop);
        layout.boosters = Rect("BoosterContainer", ui);
        Boosters(layout.boosters);
        layout.reward = Button("CoinRewardBtn",ui,Art+"Panel/btnRewardCoin.png",0,0,217,100).GetComponent<RectTransform>();
        Label("50",layout.reward,"50",118,20,70,50,44,Color.white);
        Img("Ads",layout.reward,Art+"Panel/icon video ad.png",166,-9,55,40);
        layout.banner = Rect("SampleBanner", ui);
        layout.banner.gameObject.AddComponent<Image>().color = Color.black;
        // The Figma ad is itself an image fill from a supplied source screenshot.
        // Preserve that full original and reproduce its Figma fill transform using UVs.
        var bannerImage=Rect("FigmaAdImageFill",layout.banner).gameObject.AddComponent<RawImage>();
        bannerImage.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Ref+"BannerSource.png");
        bannerImage.uvRect=new Rect(.0053555f,0,.9917683f,1f/10.55f);
        bannerImage.raycastTarget=false;
        Stretch(bannerImage.rectTransform);
        // Popups remain above the native main screen and keep their existing close buttons.
        var popups = hud.transform.Find("Popups");
        if (popups) popups.SetAsLastSibling();
        layout.Refresh();
    }

    static void World(RectTransform p)
    {
        const float S=740f/402f;
        Img("Boat_3798_3828",p,Ref+"Boat.png",197.00000f*S,195.00000f*S,194.48315f*S,156.00000f*S);
        Img("SceneMobFlying_3798_3870",p,Ref+"SceneMobFlying.png",71.12622f*S,130.98010f*S,39.74902f*S,27.01746f*S);
        Img("SceneMobFlying_3798_3871",p,Ref+"SceneMobFlying.png",150.12622f*S,145.98010f*S,39.74902f*S,27.01746f*S);
        Img("SceneMobMelee_3798_3872",p,Ref+"SceneMobMelee.png",110.00000f*S,352.09424f*S,39.65356f*S,33.90588f*S);
        Img("SceneMobMelee_3798_3873",p,Ref+"SceneMobMelee.png",10.00000f*S,351.09424f*S,39.65356f*S,33.90588f*S);
        Img("SceneMobMelee_3798_3874",p,Ref+"SceneMobMelee.png",160.00000f*S,257.09424f*S,39.65356f*S,33.90588f*S);
        Img("SceneMobMelee_3798_3875",p,Ref+"SceneMobMelee.png",62.00000f*S,343.09424f*S,39.65356f*S,33.90588f*S);
        Img("SceneMobMelee_3798_3876",p,Ref+"SceneMobMelee.png",141.00000f*S,308.09424f*S,39.65356f*S,33.90588f*S);
        Img("SceneMobRange_3798_3877",p,Ref+"SceneMobRange.png",32.00000f*S,391.00000f*S,44.00000f*S,36.33240f*S);
        Img("SceneMobRange_3798_3878",p,Ref+"SceneMobRange.png",109.00000f*S,228.00000f*S,44.00000f*S,36.33240f*S);
        Img("SceneMobRange_3798_3879",p,Ref+"SceneMobRange.png",173.00000f*S,275.00000f*S,44.00000f*S,36.33240f*S);
        Img("CharNerd_3798_3887",p,Ref+"CharNerd.png",280.45703f*S,266.45752f*S,28.08594f*S,40.23865f*S);
        Img("CharEmo_3798_3888",p,Ref+"CharEmo.png",307.43506f*S,225.43469f*S,24.89209f*S,36.68884f*S);
        Img("CharTomboy_3798_3889",p,Ref+"CharTomboy.png",337.43481f*S,231.43469f*S,29.70878f*S,43.91101f*S);
        Img("SceneMobFast_3798_3897",p,Ref+"SceneMobFast.png",169.00000f*S,338.37329f*S,47.78345f*S,44.86536f*S);
        Img("CharSexy_3798_3978",p,Ref+"CharSexy.png",311.77539f*S,272.77539f*S,34.44897f*S,37.37830f*S);
        Img("Boss_3798_4013",p,Ref+"Boss.png",-49.54006f*S,215.00000f*S,200.40829f*S,123.32818f*S);
        // Native HUD over the independently positioned world sprites.
        void Health(float x,float y,float w,float fill,Color color)
        {
            Img("HealthBack",p,Art+"Panel/slider gameplay_background.png",x*S,y*S,w*S,8*S);
            var bar=Img("HealthFill",p,Art+"Panel/white rec.png",(x+1)*S,(y+1)*S,(w-2)*fill*S,6*S);
            bar.type=Image.Type.Sliced;bar.color=color;
        }
        var red=new Color32(245,71,111,255);
        Health(16,196,97,.77f,red);Health(157,135,27,.85f,red);Health(165,245,27,.48f,red);Health(190,263,27,.93f,red);
        Health(303,169,37,.78f,new Color32(0,221,93,255));
        Label("BoatHealth",p,"120",305*S,165*S,30*S,12*S,13*S,Color.white);
        Img("Shield",p,Ref+"StatusShield.png",314*S,142*S,16*S,16*S);
        Label("ShieldValue",p,"89",309*S,152*S,24*S,12*S,13*S,Color.white);
        Img("Poison",p,Ref+"StatusPoison.png",314*S,180*S,16*S,16*S);
        Label("PoisonValue",p,"2",322*S,189*S,11*S,9*S,13*S,Color.white);
        for(int i=0;i<4;i++)
        {
            var bg=Img("Ammo",p,Art+"UI/bg_grey.png",360*S,(306+i*28)*S,53*S,22*S);
            bg.color=new Color32(32,100,147,255);
            string[] bullets={Ref+"BulletPumpkin.png",Ref+"BulletChili.png",Art+"Weapon/bullet_berry.png",Art+"Weapon/bullet_pea.png"};
            Img("Bullet",p,bullets[i],366*S,(310+i*28)*S,12*S,12*S);
            Label("AmmoCount",p,new[]{"2","0","5","13"}[i],380*S,(307+i*28)*S,22*S,20*S,13*S,Color.white);
        }
    }

    static void Header(RectTransform p, bool battle)
    {
        Label("LevelTitle",p,"Level12",255,72,230,44,33,Color.white);
        Button("SettingsBtn",p,Art+"Panel/Setting.png",17,127,55,63);
        Button("BackBtn",p,Art+"Panel/closeBtn.png",672,128,55,58);
        Img("ProgressBack",p,Art+"Panel/slider gameplay_background.png",108,148,515,20);
        Img("ProgressFill",p,Art+"Panel/slider gameplay_fill_cyan.png",111,152,214,12);
        foreach (float x in new float[] {202,310,525}) Img("Skull",p,Ref+"Skull.png",x,119,24,28);
        Img("Finish",p,Ref+"Flag.png",614,122,25,35);
        Img("Distance",p,Art+"Panel/distance pointer.png",254,163,84,34);
        Label("distanceText",p,"123m",255,168,82,23,19,Color.white);
        var hard = Img("Hard",p,Art+"UI/bg_grey.png",315,214,99,32);
        hard.color = new Color32(230,85,105,255);
        Label("difficultyText",p,"Hard",315,212,99,34,25,Color.white);
        var coin = Img("StatCoin",p,Art+"UI/bg_grey.png",0,207,143,51);
        coin.color = new Color32(43,64,91,255);
        Img("Sun",p,Art+"Icon/icon_sun.png",9,202,52,53);
        Label("textCoin",p,"165",65,208,73,47,28,new Color32(255,220,43,255),false);
        var x2 = Button("SpeedBtn",p,Ref+"Speed.png",673,204,53,53);
        Label("Text",x2.transform,"x2",2,5,49,38,25,Color.white);
    }

    static void Board(RectTransform p, bool battle)
    {
        float w = battle ? 570 : 698;
        var panel = Img("Panel",p,Ref+"BoardPanel.png",0,0,w,700);
        Stretch(panel.rectTransform);
        panel.type = Image.Type.Sliced;
        panel.pixelsPerUnitMultiplier = 2.8f;
        // All contents use 698-wide coordinates. The combat board scales the same layout down.
        var contents = Rect("Contents",p);
        contents.anchorMin=contents.anchorMax=new Vector2(0,1);
        contents.pivot=new Vector2(0,1);
        contents.anchoredPosition=new Vector2(0,0);
        contents.sizeDelta=new Vector2(698,700);
        contents.localScale=Vector3.one*(w/698);
        var section=Img("EquipmentBackground",contents,Art+"Panel/white rec.png",18,18,662,225);
        section.type=Image.Type.Sliced;section.pixelsPerUnitMultiplier=3;
        section.color=new Color32(108,168,137,255);
        var speed=Img("StatSpeed",contents,Art+"Panel/white rec.png",37,26,310,50);
        speed.type=Image.Type.Sliced;speed.pixelsPerUnitMultiplier=4;
        speed.color=battle?new Color32(173,58,43,255):new Color32(42,60,94,255);
        var power=Img("StatDamage",contents,Art+"Panel/white rec.png",354,26,312,50);
        power.type=Image.Type.Sliced;power.pixelsPerUnitMultiplier=4;
        power.color=battle?new Color32(0,143,38,255):new Color32(42,60,94,255);
        Img("SpeedIcon",contents,Art+"Icon/icon_move speed.png",46,34,43,36);
        Img("DamageIcon",contents,Art+"Icon/icon_fire rate.png",369,34,46,35);
        Label("textSpeed",contents,"52.3mph",102,31,191,42,24,Color.white,false);
        Label("PowerText",contents,"73.3",421,31,191,42,24,Color.white,false);
        if (battle)
        {
            Img("Down",contents,Art+"UI/arrow down.png",295,41,29,20);
            Img("Up",contents,Art+"UI/arrow up.png",621,41,29,20);
        }
        string[] weapons={"boat wheel.png","Gun_pea_slot.png","gun_berry_slot.png",null,null,"boat sail.png"};
        for(int i=0;i<6;i++)
        {
            float x=37+i*106;
            Img("WeaponSlot"+i,contents,Ref+"WeaponSlotDisabled.png",x,85,95,137);
            if(weapons[i]!=null)
            {
                Img("WeaponSlot"+i,contents,i<2?Ref+"WeaponSlotActive.png":Ref+"WeaponSlotEmpty.png",x,85,95,137);
                Img("Weapon",contents,Art+"Weapon/"+weapons[i],x+4,98,85,92).preserveAspect=true;
                if(i<3) Label("Rate",contents,i==0?"0.24/s":(i==1?"1.17/s":""),x,176,93,24,20,Color.white);
            }
            if(weapons[i]!=null)
                Img("Connected",contents,i<2?Ref+"ConnectorHighlight.png":Ref+"Connector.png",x+18,197,60,60);
        }
        if(battle)
        {
            for(int i=1;i<=2;i++)
            {
                float x=37+i*106+8;
                var buff=Img("WeaponBuff",contents,Art+"Panel/white rec.png",x,77,77,32);
                buff.type=Image.Type.Sliced;buff.pixelsPerUnitMultiplier=5;
                buff.color=new Color32(42,60,94,179);
                Img("Bullet",contents,Art+"Weapon/"+(i==1?"bullet_pea.png":"bullet_berry.png"),x+4,80,27,27);
                Label("BuffValue",contents,i==1?"+1":"+2",x+28,78,47,28,22,Color.white);
            }
        }
        var pads=Rect("Grid",contents);
        // Measured independently in the two PPT screenshots, then normalized to a 698-wide board.
        float pitchX=battle?107.6f:106.6f, pitchY=battle?107.6f:104.7f;
        Pos(pads,battle?26.2f:38.8f,battle?266.9f:259.9f,637,418);
        Image Cell(string name,string path,int c,int r,float width=93,float height=93,float dx=0,float dy=0)
        {
            return Img(name,pads,path,c*pitchX+46.5f-width*.5f+dx,
                r*pitchY+46.5f-height*.5f+dy,width,height);
        }
        void Flower(string name,int c,int r)
        {
            string body="GearWeapon";
            string number=null,icon=null;
            switch(name)
            {
                case "GearOrange2":body="Gear2";number="2";break;
                case "GearRed8":body="Gear8";number="8";break;
                case "GearCyan4":body="Gear4";number="4";break;
                case "GearPurple1":body="Gear1";number="1";break;
                case "GearPinkSmile":icon="HeartBoom";break;
                case "GearPinkPotato":icon=battle?"PeanutDrone2":"SunbloomBurst";break;
                case "GearDamage":body="GearStats";icon="FlowerCritDmg";break;
                case "GearSunPlus":body="GearEarning";icon=battle?"FlowerExtraGold":"FlowerRegeneration";break;
                case "GearShield":body="GearStats";icon="FlowerShield";break;
            }
            float g=battle?113:112,dx=battle?5:0,dy=battle?-15:-3;
            Cell(name,Ref+body+"_Full.png",c,r,g,g,dx,dy);
            if(number!=null)
                Label("Number",pads,number,c*pitchX+46.5f-g*.5f+dx,r*pitchY+46.5f-g*.5f+dy,g,g,65,Color.white);
            if(icon!=null)
            {
                float iw=icon=="PeanutDrone2"?79:icon=="HeartBoom"?79:84;
                Cell("Icon_"+icon,Ref+icon+"_Full.png",c,r,iw,iw,dx,dy);
            }
        }
        // Inactive pad is the lighter green leaf, including empty cells behind gears.
        for(int r=0;r<4;r++)for(int c=0;c<6;c++)
        {
            bool active=(r==0&&c==2)||(r==1&&c==1)||(r==2&&c<3);
            var leaf=Cell("Pad_"+r+"_"+c,Ref+(active?"PadEnabled.png":"Pad.png"),c,r);
            leaf.rectTransform.pivot = new Vector2(.5f,.5f);
            leaf.rectTransform.anchoredPosition += new Vector2(46.5f,-46.5f);
            leaf.rectTransform.localEulerAngles=new Vector3(0,0,(r*83+c*47)%360);
        }
        Flower("GearOrange2",0,0); Flower("GearRed8",1,0); Flower(battle?"GearPinkPotato":"GearPinkSmile",4,0);
        Flower("GearCyan4",0,1); Flower("GearPurple1",2,1); Flower("GearCyan4",3,1); Flower("GearPurple1",4,1);
        // The rotor body occupies only 164/232 of its source texture, so its rect must be larger.
        var wheel=Cell("Rotor",Ref+"GearRotor_Full.png",1,1,136,136,0,battle?-1.6f:5.3f);
        wheel.rectTransform.pivot = new Vector2(.5f,.5f);
        wheel.rectTransform.anchoredPosition += new Vector2(68,-68);
        wheel.rectTransform.localEulerAngles=new Vector3(0,0,-135);
        Flower("GearSunPlus",0,3);
        if (battle)
        {
            Flower("GearDamage",0,2);
            Flower("GearOrange2",3,2);
            var freeze=Rect("PartialFrozen",pads);Pos(freeze,3*pitchX-5,2*pitchY+39,113,57);
            freeze.gameObject.AddComponent<RectMask2D>();
            Img("Ice",freeze,Ref+"GearFrozen_Full.png",0,-56,113,113);
            Cell("MultiplierGear",Ref+"GearMultiplier.png",2,3,113,113,5,-15);
            Cell("MultiplierIcon",Ref+"MultiplierIcon.png",2,3,87,47,5,-15);
            Cell("FrozenMultiplier",Ref+"GearFrozen_Full.png",2,3,113,113,5,-15);
            Cell("WeaponFlower",Ref+"GearWeapon_Full.png",3,3,113,113,5,-15);
            Cell("CombatPlant",Ref+"GearSpider_Full.png",3,0,113,113,5,-15);
            Cell("CombatWeaponGear",Ref+"Gear2_Full.png",4,2,113,113,5,-15);
            Label("Number",pads,"1",4*pitchX-5,2*pitchY-25,113,113,65,Color.white);
            Cell("CombatWeapon",Ref+"GearAd_Full.png",4,2,113,113,5,-15);
            Cell("CombatStone",Ref+"StonePhase1_Full.png",5,3,107,107,5,-15);
        }
        else
        {
            Flower("GearOrange2",3,2); Flower("GearPurple1",4,2); Flower("GearPinkPotato",3,3);
            Cell("BlueFlower",Ref+"GearStats_Full.png",5,3,112,112,0,-3);
            Cell("Luck",Ref+"FlowerLuck_Full.png",5,3,84,84,0,-3);
        }
    }

    static void Shop(RectTransform p)
    {
        var bg=Img("Panel",p,Ref+"BuyBar.png",0,0,740,364);
        Stretch(bg.rectTransform);
        for(int i=0;i<4;i++)
        {
            float x=29+i*177;
            Img("Offer"+i,p,Ref+"ButtonBuy.png",x,43,148,150);
            if(i==3)continue;
            Img("Gear",p,Ref+new[]{"Gear1_Full.png","Gear2_Full.png","Gear4_Full.png"}[i],x+24,64,105,105);
            Label("Number",p,new[]{"1","2","4"}[i],x+24,64,105,105,61,Color.white);
            var price=Img("Price",p,Art+"UI/bg_grey.png",x+19,151,112,39);
            price.color=new Color32(67,70,101,210);
            Img("Sun",p,Art+"Icon/icon_sun.png",x+24,153,29,29);
            Label("price_text",p,i==0?"5":i==1?"10":"Free",x+55,151,75,36,26,Color.white,false);
            if(i==2)
            {
                Img("Info",p,Art+"Panel/info.png",x+1,41,40,40);
                Img("Ads",p,Art+"Panel/icon video ad.png",x+98,40,57,40);
            }
        }
        var strip=Img("ButtonStrip",p,Art+"Panel/white rec.png",27,212,686,136);
        strip.type=Image.Type.Sliced;
        strip.color=new Color32(109,73,182,255);
        var refresh=Button("RefreshBtn",p,Ref+"ButtonGreen.png",113,233,185,99);
        Label("Label",refresh.transform,"Refresh",4,5,177,43,28,Color.white);
        Img("Sun",refresh.transform,Art+"Icon/icon_sun.png",60,50,33,34);
        Label("Price",refresh.transform,"5",91,48,42,37,29,Color.white);
        var start=Button("StartGameBtn",p,Ref+"ButtonBigCyan.png",314,233,318,99);
        Label("Label",start.transform,"START!",0,8,318,79,40,Color.white);
    }

    static void Boosters(RectTransform p)
    {
        for(int i=0;i<4;i++)
        {
            float y=i*135;
            var b=Button("Booster"+i,p,Ref+"BoosterFrame.png",0,y,112,112);
            if(i==0) Img("Heart",b.transform,Art+"Icon/icon_heart.png",23,21,68,63);
            var badge=Img("Count",b.transform,Art+"Panel/dot_green.png",79,77,41,41);
            Label("countText",badge.transform,"3",0,0,41,39,30,Color.white);
        }
    }

    static RectTransform Rect(string name,Transform p)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(p,false); r.localScale=Vector3.one; return r;
    }
    static Image Img(string name,Transform p,string path,float x,float y,float w,float h)
    {
        var rt=Rect(name,p); Pos(rt,x,y,w,h);
        var im=rt.gameObject.AddComponent<Image>(); im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path); im.raycastTarget=false;
        if(!im.sprite)Debug.LogError("Missing reference sprite: "+path);
        return im;
    }
    static Button Button(string name,Transform p,string path,float x,float y,float w,float h)
    {
        var im=Img(name,p,path,x,y,w,h); im.raycastTarget=true;
        var b=im.gameObject.AddComponent<Button>(); b.targetGraphic=im; return b;
    }
    static TMP_Text Label(string name,Transform p,string value,float x,float y,float w,float h,float size,Color color,bool outline=true)
    {
        var rt=Rect(name,p);Pos(rt,x,y,w,h);
        var t=rt.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font;t.text=value;t.fontSize=size;t.color=color;
        t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.enableWordWrapping=false;
        if(outline){t.outlineColor=Ink;t.outlineWidth=.16f;}
        return t;
    }
    static void Pos(RectTransform r,float x,float y,float w,float h)
    {
        r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
    }
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}

    [MenuItem("Combat UI/Capture Full Screens")]
    public static void CaptureFullScreens()
    {
        EditorSceneManager.OpenScene("Assets/CombatUI/CombatUI_Demo.unity");
        var preview=Object.FindObjectOfType<UIPreviewSwitcher>();
        var cam=Camera.main;
        Directory.CreateDirectory("Screenshots");
        for(int ratio=0;ratio<3;ratio++)for(int state=0;state<2;state++)
        {
            // The slide images have slightly different portrait dimensions. Keep each exact ratio.
            int width=ratio==0?1080:ratio==1?1536:state==0?1116:1124;
            int height=ratio==0?2520:ratio==1?2048:state==0?2436:2444;
            string prefix=ratio==0?"r921":ratio==1?"r34":"slide";
            preview.ShowState(state);preview.ShowPopup(-1);
            var hud=preview.states[state].hud;
            hud.GetComponent<ResponsiveCanvasScaler>().enabled=false;
            hud.GetComponent<CanvasScaler>().enabled=false;
            hud.GetComponent<Canvas>().scaleFactor=Mathf.Min(width/740f,height/1616f);
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;
            for(int k=0;k<3;k++){Canvas.ForceUpdateCanvases();hud.GetComponent<AssignmentResponsiveLayout>().Refresh();}
            cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes("Screenshots/"+prefix+"_"+(state==0?"buy":"battle")+".png",tex.EncodeToPNG());
            RenderTexture.active=null;cam.targetTexture=null;Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);
        }
        // Reload to discard temporary rendering settings without modifying the saved scene.
        EditorSceneManager.OpenScene("Assets/CombatUI/CombatUI_Demo.unity");
        Debug.Log("ASSIGNMENT_FULL_SCREEN_CAPTURE_COMPLETE");
    }

    public static void VerifyAndCapture()
    {
        CombatUIDemoBuilder.Build();
        CaptureFullScreens();
        Debug.Log("ASSIGNMENT_UI_VERIFY_COMPLETE");
    }

    public static void Verify()
    {
        CombatUIDemoBuilder.Build();
        var preview=Object.FindObjectOfType<UIPreviewSwitcher>();
        var cam=Camera.main;
        Directory.CreateDirectory("Screenshots");
        for(int ratio=0;ratio<2;ratio++)for(int state=0;state<2;state++)
        {
            int width=ratio==0?1080:1536,height=ratio==0?2520:2048;
            preview.ShowState(state);preview.ShowPopup(-1);
            var hud=preview.states[state].hud;
            hud.GetComponent<ResponsiveCanvasScaler>().enabled=false;
            hud.GetComponent<CanvasScaler>().enabled=false;
            hud.GetComponent<Canvas>().scaleFactor=Mathf.Min(width/740f,height/1616f);
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;
            for(int k=0;k<3;k++){Canvas.ForceUpdateCanvases();hud.GetComponent<AssignmentResponsiveLayout>().Refresh();}
            cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes("Screenshots/"+(ratio==0?"r921":"r34")+"_"+(state==0?"buy":"battle")+".png",tex.EncodeToPNG());
            RenderTexture.active=null;cam.targetTexture=null;Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);
            hud.GetComponent<ResponsiveCanvasScaler>().enabled=true;hud.GetComponent<CanvasScaler>().enabled=true;
        }
        // Save production settings, not temporary render scale factors.
        CombatUIDemoBuilder.Build();
        Debug.Log("ASSIGNMENT_UI_VERIFY_COMPLETE");
    }

    static int smokeFrames;
    public static void Smoke()
    {
        EditorSceneManager.OpenScene("Assets/CombatUI/CombatUI_Demo.unity");
        EditorApplication.playModeStateChanged += change =>
        {
            if(change==PlayModeStateChange.EnteredPlayMode)
            {smokeFrames=0;EditorApplication.update+=SmokeTick;}
        };
        // Persist callback across domain reload using a SessionState-backed initializer.
        SessionState.SetBool("AssignmentSmokePending",true);
        EditorApplication.isPlaying=true;
    }

    [InitializeOnLoadMethod]
    static void ResumeSmoke()
    {
        if(!SessionState.GetBool("AssignmentSmokePending",false))return;
        EditorApplication.playModeStateChanged += change =>
        {
            if(change==PlayModeStateChange.EnteredPlayMode)
            {smokeFrames=0;EditorApplication.update+=SmokeTick;}
        };
    }
    static void SmokeTick()
    {
        if(!EditorApplication.isPlaying || ++smokeFrames<20)return;
        EditorApplication.update-=SmokeTick;
        SessionState.SetBool("AssignmentSmokePending",false);
        try
        {
            var p=Object.FindObjectOfType<UIPreviewSwitcher>();
            var buy=p.states[0].hud.transform;
            if(p.currentState!=0)throw new System.Exception("Expected gear state on load");
            buy.Find("ReferenceUI/ShopContainer/StartGameBtn").GetComponent<Button>().onClick.Invoke();
            if(p.currentState!=1 || !p.states[1].hud.activeSelf || p.states[0].hud.activeSelf)
                throw new System.Exception("START did not switch to combat");
            var hud=p.states[1].hud.transform;
            hud.Find("ReferenceUI/LevelProgressBar/SettingsBtn").GetComponent<Button>().onClick.Invoke();
            var settings=hud.Find("Popups/SettingsPopup").GetComponent<CanvasGroup>();
            if(settings.alpha<.99f || !settings.blocksRaycasts)throw new System.Exception("Settings did not open");
            hud.Find("Popups/SettingsPopup/BG/CloseBtn").GetComponent<Button>().onClick.Invoke();
            if(settings.alpha>.01f || settings.blocksRaycasts)throw new System.Exception("Settings did not close");
            hud.Find("ReferenceUI/LevelProgressBar/BackBtn").GetComponent<Button>().onClick.Invoke();
            hud.Find("Popups/PopupGiveUp/GiveUpUI/Button/Exit").GetComponent<Button>().onClick.Invoke();
            if(p.currentState!=0)throw new System.Exception("Quit did not restore gear state");
            Debug.Log("ASSIGNMENT_UI_SMOKE_COMPLETE");EditorApplication.Exit(0);
        }
        catch(System.Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
#endif
