using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WordGarden
{
    public sealed class GameController : MonoBehaviour
    {
        static readonly Color Navy = Hex("#172D42"), Cream=Hex("#FFF7E8"), Mint=Hex("#80E3C3"),
            Sky=Hex("#B6E9EE"), Plum=Hex("#645C96"), Yellow=Hex("#FFD56F"), Red=Hex("#F58D81");
        Canvas canvas; RectTransform root; Font font; LearningEngine learning; DuelState duel;
        Speech speech = new Speech(); bool versus; int feedback; string feedbackText;
        float feedbackUntil; Challenge current; bool walking; Vector2 walkPosition; RectTransform walkingFox; RectTransform walkingShadow; Image foxImage; Sprite idleFrame; Sprite blinkFrame; Sprite[] walkFrames; float strideClock; RectTransform[] midLayers; RectTransform[] nearLayers; Vector2 touchDirection; List<string> sentenceTiles = new List<string>(); List<int> selectedTileIndices = new List<int>(); Button[] tileButtons;

        void Awake()
        {
            learning = new LearningEngine(LearningEngine.Load());
            font = Resources.Load<Font>("Fonts/DejaVuSans");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var c = new GameObject("Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            c.transform.SetParent(transform,false); canvas=c.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=1;
            var scaler=c.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=1f; // Keep all answer buttons inside the viewport in landscape.
            root=c.GetComponent<RectTransform>();
            var events = new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule)); events.transform.SetParent(transform);
            Application.targetFrameRate=60;
            idleFrame=Art.Load("fox_idle") ?? Art.Load("fox"); blinkFrame=Art.Load("fox_blink");
            walkFrames=new[] {Art.Load("fox_walk_0"),Art.Load("fox_walk_1"),Art.Load("fox_walk_2"),Art.Load("fox_walk_1")};
            ShowHome();
        }
        void OnDestroy() { speech.Dispose(); }
        void Update()
        {
            if (walking && walkingFox != null)
            {
                Vector2 keys = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                Vector2 direction = (keys + touchDirection).normalized;
                bool moving = direction.sqrMagnitude > .01f;
                if (moving)
                {
                    walkPosition += direction * (460f * Time.unscaledDeltaTime);
                    walkPosition.y = Mathf.Clamp(walkPosition.y, -520f, 160f);
                    // The walkable trail gets narrower toward the garden gate.
                    float width = Mathf.Lerp(240f, 95f, Mathf.InverseLerp(-520f, 160f, walkPosition.y));
                    walkPosition.x = Mathf.Clamp(walkPosition.x, -width, width);
                    strideClock += Time.unscaledDeltaTime * 8.5f;
                    int pose = Mathf.FloorToInt(strideClock) % walkFrames.Length;
                    if (foxImage != null) foxImage.sprite = walkFrames[pose] ?? idleFrame;
                    var scale = walkingFox.localScale;
                    scale.x = direction.x < -.1f ? -1f : direction.x > .1f ? 1f : scale.x;
                    walkingFox.localScale = scale;
                }
                else
                {
                    strideClock = 0f;
                    if (foxImage != null) foxImage.sprite = blinkFrame != null && Time.unscaledTime % 4.8f < .18f ? blinkFrame : idleFrame;
                }
                // Animated frames change actual paw and arm poses; breathing remains in idle.
                float bob = moving ? Mathf.Sin(strideClock * Mathf.PI) * 5f : Mathf.Sin(Time.unscaledTime * 2.5f) * 4f;
                walkingFox.anchoredPosition = walkPosition + new Vector2(0,bob);
                float breath = moving ? 1f : 1f + .014f * Mathf.Sin(Time.unscaledTime * 2.5f);
                walkingFox.localScale = new Vector3(Mathf.Sign(walkingFox.localScale.x)*breath,breath,1f);
                if (walkingShadow != null)
                {
                    walkingShadow.anchoredPosition = walkPosition + new Vector2(0,-172);
                    walkingShadow.localScale=new Vector3(moving ? 1f+.045f*Mathf.Sin(strideClock*Mathf.PI) : 1f,1f,1f);
                }
                MoveGardenLayers();
                if (moving && walkPosition.y >= 145f) { walking = false; touchDirection = Vector2.zero; ShowChallenge(); }
            }
            if (feedback!=0 && Time.unscaledTime>feedbackUntil)
            {
                int was=feedback; feedback=0;
                if (was==1) { if (versus && duel.finished) ShowResult(); else ShowWalk(true); }
                else if (versus) { if (duel.finished) ShowResult(); else ShowWalk(true); }
                else ShowChallenge(); // Retry the same item immediately instead of another long walk.
            }
        }
        static Color Hex(string h) { ColorUtility.TryParseHtmlString(h,out Color c); return c; }
        static string Hebrew(string raw)
        {
            // Legacy uGUI lacks BiDi shaping. All Hebrew strings here are isolated RTL lines;
            // reverse each line for display. English words live in separate text fields.
            if (string.IsNullOrEmpty(raw)) return raw;
            var a=raw.ToCharArray(); Array.Reverse(a); return new string(a);
        }
        RectTransform Panel(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int radius=0)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
            r.sizeDelta=size;r.anchoredPosition=pos;
            var img=go.GetComponent<Image>();img.color=color;if(radius>0) img.sprite=Art.Rounded;img.type=radius>0?Image.Type.Sliced:Image.Type.Simple;
            return r;
        }
        Text Label(string name, Transform parent,string text, Vector2 pos,Vector2 size,int px,Color color,bool rtl=false,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=pos;
            var t=go.GetComponent<Text>();t.font=font;t.text=rtl?Hebrew(text):text;t.fontSize=px;t.fontStyle=FontStyle.Bold;
            t.color=color;t.alignment=align;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
            return t;
        }
        Button Action(string name,Transform parent,string caption,Vector2 pos,Vector2 size,Color bg,Color fg,Action onClick,bool rtl=true,int px=42)
        {
            var shadow=Panel(name+" shadow",parent,pos+new Vector2(0,-9),size,new Color(0,.08f,.16f,.17f),1);
            var r=Panel(name,parent,pos,size,bg,1);var b=r.gameObject.AddComponent<Button>(); b.targetGraphic=r.GetComponent<Image>();
            b.onClick.AddListener(()=>onClick());Label(name+" label",r,caption,Vector2.zero,size-new Vector2(32,12),px,fg,rtl);
            return b;
        }
        void Clear(bool darken=true)
        {
            for (int i=root.childCount-1;i>=0;i--) Destroy(root.GetChild(i).gameObject);
            // Safe portrait canvas margin. Touch targets remain >= 120 reference px.
            var backdrop=Panel("Backdrop",root,Vector2.zero,new Vector2(1080,1920),Navy);
            var world=Art.Load("world");
            if (world!=null)
            {
                var image=backdrop.GetComponent<Image>(); image.sprite=world; image.color=Color.white;
                image.preserveAspect=false;
            }
            if (darken) Panel("Readability veil",root,new Vector2(0,-260),new Vector2(1080,1500),new Color(.045f,.10f,.20f,.67f),1);
        }
        void Heading(string badge, string title, string sub)
        {
            Label("Brand",root,"✦  WORD GARDEN",new Vector2(0,845),new Vector2(960,70),38,Mint);
            Panel("Badge",root,new Vector2(0,690),new Vector2(410,78),Yellow,1);
            Label("Unit",root,badge,new Vector2(0,690),new Vector2(380,70),28,Navy,true);
            Label("Title",root,title,new Vector2(0,570),new Vector2(960,128),64,Cream,true);
            if (!string.IsNullOrEmpty(sub)) Label("Subtitle",root,sub,new Vector2(0,477),new Vector2(930,74),31,Sky,true);
        }
        void ShowHome()
        {
            walking = false; Clear(false);
            Panel("Top shade",root,new Vector2(0,725),new Vector2(1080,470),new Color(.05f,.17f,.19f,.65f),1);
            Heading("משחק ולומדים ביחד", "ממלכת המילים", "יוצאים לטייל, מגלים מילים חדשות.");
            var card=Panel("Mascot stage",root,new Vector2(0,95),new Vector2(850,430),new Color(.08f,.18f,.23f,.43f),1);
            Panel("Mascot halo",card,new Vector2(0,65),new Vector2(370,370),new Color(1f,.82f,.43f,.48f),1);
            var tex=Art.Load("fox"); if (tex!=null){var icon=Panel("Fox",card,new Vector2(0,22),new Vector2(340,340),Color.white);var img=icon.GetComponent<Image>();img.sprite=tex;img.preserveAspect=true;}
            Label("Hook",card,"WORD GARDEN",new Vector2(0,-170),new Vector2(750,70),44,Cream);
            Panel("Bottom shade",root,new Vector2(0,-510),new Vector2(1080,850),new Color(.06f,.18f,.20f,.68f),1);
            Label("Status",root,$"{learning.Memory.mastered.Count} מילים ומשימות נלמדו",new Vector2(0,-186),new Vector2(890,80),31,Cream,true);
            Action("Solo",root,"מסע לימוד",new Vector2(0,-348),new Vector2(800,130),Mint,Navy,()=>{versus=false;ShowWalk(true);});
            Action("Duel",root,"דו קרב על אותו מכשיר",new Vector2(0,-520),new Vector2(800,130),Yellow,Navy,()=>{versus=true;duel=new DuelState();ShowWalk(true);},true,36);
            Label("Limit",root,"הדגמת למידה מקומית • בלי חשבון ובלי פרסומות",new Vector2(0,-745),new Vector2(940,90),26,Cream,true);
        }
        RectTransform GardenLayer(string name, Vector2 offset)
        {
            var frame=Art.Load(name);
            if(frame==null) return null;
            var layer=Panel(name,root,offset,new Vector2(1080,1920),Color.white);
            var image=layer.GetComponent<Image>();image.sprite=frame;image.preserveAspect=false;image.raycastTarget=false;
            return layer;
        }
        void MoveGardenLayers()
        {
            float x=walkPosition.x / 240f;
            float y=Mathf.InverseLerp(-520f,160f,walkPosition.y);
            if(midLayers != null) foreach(var layer in midLayers)
                if(layer!=null) layer.anchoredPosition=new Vector2(-x*20f,-y*16f);
            if(nearLayers != null) foreach(var layer in nearLayers)
                if(layer!=null) layer.anchoredPosition=new Vector2(-x*55f,-y*43f);
        }
        void ShowWalk(bool freshStage)
        {
            walking = true; touchDirection = Vector2.zero;
            if (freshStage) walkPosition = new Vector2(0,-510);
            current = versus ? duel.Challenge : learning.Current;
            Clear(false);
            midLayers=new[] {GardenLayer("garden_mid_left",Vector2.zero),GardenLayer("garden_mid_right",Vector2.zero)};
            Panel("Sky ribbon",root,new Vector2(0,802),new Vector2(1080,310),new Color(.05f,.17f,.20f,.72f),1);
            Label("Walk brand",root,"WORD GARDEN  ✦",new Vector2(0,850),new Vector2(800,65),37,Cream);
            Label("Walk unit",root,versus?$"שחקן {duel.player+1}  •  {current.unit}":current.unit,
                new Vector2(0,760),new Vector2(860,80),36,Yellow,true);
            // The glowing gate marks the next question. Walking is required to reach it.
            Panel("Station glow",root,new Vector2(0,160),new Vector2(320,250),new Color(1f,.79f,.30f,.35f),1);
            Panel("Station sign",root,new Vector2(0,160),new Vector2(245,172),Cream,1);
            Label("Station question",root,"?",new Vector2(0,172),new Vector2(160,142),94,Plum);
            Label("Station label",root,"תחנת מילים",new Vector2(0,52),new Vector2(530,70),31,Navy,true);
            walkingShadow=Panel("Fox shadow",root,walkPosition+new Vector2(0,-172),new Vector2(215,49),new Color(.09f,.13f,.09f,.28f));
            walkingShadow.GetComponent<Image>().sprite=Art.Load("soft_shadow");
            walkingShadow.GetComponent<Image>().raycastTarget=false;
            walkingFox=Panel("Walking fox",root,walkPosition,new Vector2(285,360),Color.white);
            foxImage=walkingFox.GetComponent<Image>(); foxImage.sprite=idleFrame; foxImage.preserveAspect=true;
            foxImage.raycastTarget=false; strideClock=0f;
            nearLayers=new[] {GardenLayer("garden_near_left",Vector2.zero),GardenLayer("garden_near_right",Vector2.zero)};
            MoveGardenLayers();
            Label("Walk hint",root,"לכו עם החיצים עד לתחנת המילים",new Vector2(0,-635),new Vector2(870,82),33,Cream,true);
            Panel("Controls shade",root,new Vector2(0,-815),new Vector2(1080,290),new Color(.05f,.17f,.20f,.66f),1);
            Direction("Left", "◀",new Vector2(-332,-824),Vector2.left);
            Direction("Right","▶",new Vector2(-110,-824),Vector2.right);
            Direction("Up", "▲",new Vector2(222,-824),Vector2.up);
            Direction("Down","▼",new Vector2(440,-824),Vector2.down);
            Action("Walk home",root,"⌂",new Vector2(-445,830),new Vector2(98,92),Cream,Navy,ShowHome,false,39);
        }
        void Direction(string name,string symbol,Vector2 pos,Vector2 direction)
        {
            var button=Action(name,root,symbol,pos,new Vector2(174,120),Cream,Navy,()=>{},false,48);
            var trigger=button.gameObject.AddComponent<EventTrigger>();
            var down=new EventTrigger.Entry { eventID=EventTriggerType.PointerDown };
            down.callback.AddListener(_=>touchDirection=direction); trigger.triggers.Add(down);
            var up=new EventTrigger.Entry { eventID=EventTriggerType.PointerUp };
            up.callback.AddListener(_=>touchDirection=Vector2.zero); trigger.triggers.Add(up);
            var exit=new EventTrigger.Entry { eventID=EventTriggerType.PointerExit };
            exit.callback.AddListener(_=>touchDirection=Vector2.zero); trigger.triggers.Add(exit);
        }
        void ShowChallenge()
        {
            walking=false; current=versus?duel.Challenge:learning.Current;
            sentenceTiles.Clear(); selectedTileIndices.Clear();
            Clear(); Heading(current.unit,versus?"דו קרב מילים":"המסע שלי",versus?$"שחקן {duel.player+1} • סיבוב {duel.round+1} מתוך {duel.maxRounds}":"מגלים • מתרגלים • חוזרים");
            Panel("Progress track",root,new Vector2(0,386),new Vector2(820,26),Hex("#547088"),1);
            var progress=versus?(float)duel.round/duel.maxRounds:(float)learning.Memory.lessonsCompleted/Curriculum.All.Length;
            var width=Mathf.Max(24,820*progress);Panel("Progress",root,new Vector2(-410+width/2,386),new Vector2(width,26),Mint,1);
            Panel("Challenge shadow",root,new Vector2(0,45),new Vector2(905,580),new Color(0f,.02f,.09f,.48f),1);
            var card=Panel("Challenge card",root,new Vector2(0,70),new Vector2(880,560),Cream,1);
            Label("Instruction",card,current.instruction,new Vector2(0,225),new Vector2(820,80),32,Navy,true);
            if (!string.IsNullOrEmpty(current.illustration))
            {
                Panel("Illustration backing",card,new Vector2(0,35),new Vector2(290,290),Sky,1);
                var im=Panel("Illustration",card,new Vector2(0,35),new Vector2(270,270),Color.white);
                var pic=im.GetComponent<Image>();pic.sprite=current.illustration=="fox" ? idleFrame : Art.Load(current.illustration);pic.preserveAspect=true;
            }
            else
            {
                Label("Ears",card,"♫",new Vector2(0,45),new Vector2(300,260),155,Plum);
            }
            Label("Prompt",card,current.prompt,new Vector2(0,-208),new Vector2(790,74),36,Navy,true);
            Action("Hear",root,"▶  הקש לשמוע",new Vector2(0,-307),new Vector2(560,94),Plum,Cream,()=>{
                if (!speech.Speak(current.spoken)) Label("Audio fallback",root,current.spoken,new Vector2(0,-377),new Vector2(700,70),34,Yellow);
            },true,30);
            if (current.kind==TaskKind.Sentence)
            {
                Label("Built sentence",root, "Tap words in order",new Vector2(0,-410),new Vector2(830,80),32,Cream);
                tileButtons=new Button[current.choices.Length];
                int[] positions={2,0,3,1}; // predictable for tests, not answer order
                for (int i=0;i<current.choices.Length;i++)
                {
                    int index=positions[i];
                    tileButtons[index]=Action("Tile "+index,root,current.choices[index],new Vector2(-302+i*202,-535),new Vector2(184,112),Mint,Navy,()=>AddWord(index),false,29);
                }
                Action("Undo tile",root,"חזור",new Vector2(-205,-715),new Vector2(360,105),Sky,Navy,()=>{
                    if(sentenceTiles.Count>0){int last=selectedTileIndices[selectedTileIndices.Count-1];selectedTileIndices.RemoveAt(selectedTileIndices.Count-1);sentenceTiles.RemoveAt(sentenceTiles.Count-1);tileButtons[last].interactable=true;RefreshTiles();}
                });
                Action("Submit sentence",root,"בדוק משפט",new Vector2(205,-715),new Vector2(360,105),Yellow,Navy,()=>{if(sentenceTiles.Count==current.choices.Length) Choose(string.Join(" ",sentenceTiles));},true,33);
            }
            else for (int i=0;i<current.choices.Length;i++)
            {
                string choice=current.choices[i];
                Action("Choice "+i,root,choice,new Vector2(0,-464-i*146),new Vector2(840,119),i%2==0?Mint:Sky,Navy,()=>Choose(choice),false,choice.Length>18?31:40);
            }
            Action("Home",root,"⌂",new Vector2(-429,841),new Vector2(100,95),Hex("#45637B"),Cream,ShowHome,false,40);
        }
        void AddWord(int index)
        {
            if (sentenceTiles.Count>=current.choices.Length) return;
            if(selectedTileIndices.Contains(index)) return;
            selectedTileIndices.Add(index); sentenceTiles.Add(current.choices[index]);
            tileButtons[index].interactable=false; RefreshTiles();
        }
        void RefreshTiles()
        {
            var text=root.Find("Built sentence")?.GetComponent<Text>();
            if(text!=null) text.text=sentenceTiles.Count==0?"Tap words in order":string.Join(" ",sentenceTiles);
        }
        void Choose(string choice)
        {
            if (feedback!=0) return;
            bool correct;
            if (versus){correct=choice==current.answer;duel.Play(choice);}
            else {correct=learning.Submit(current,choice);learning.Save();}
            feedback=correct?1:2;
            feedbackText=correct?"כל הכבוד!":"כמעט! ננסה שוב עוד רגע";
            feedbackUntil=Time.unscaledTime+1.6f;
            Panel("Feedback scrim",root,Vector2.zero,new Vector2(1100,1920),new Color(.07f,.17f,.25f,.88f));
            Panel("Feedback card",root,Vector2.zero,new Vector2(850,390),correct?Mint:Yellow,1);
            Label("Feedback",root,feedbackText,new Vector2(0,42),new Vector2(780,125),53,Navy,true);
            Label("Answer",root,correct?current.answer:"התשובה: "+current.answer,new Vector2(0,-71),new Vector2(800,110),35,Navy,!correct);
        }
        void ShowResult()
        {
            Clear(); Heading("המסע ממשיך", "כל סיבוב מלמד", "לא מפסידים ידע. חוזרים ומצליחים.");
            Panel("Results card",root,new Vector2(0,125),new Vector2(870,520),Cream,1);
            Label("Score",root,$"{duel.scoreA}  :  {duel.scoreB}",new Vector2(0,170),new Vector2(800,190),115,Plum);
            Label("Players",root,"שחקן 1                 שחקן 2",new Vector2(0,50),new Vector2(850,90),36,Navy,true);
            Action("Again",root,"עוד משחק",new Vector2(0,-350),new Vector2(800,130),Mint,Navy,()=>{duel=new DuelState();ShowWalk(true);});
            Action("Back",root,"בחזרה לבית",new Vector2(0,-530),new Vector2(800,130),Yellow,Navy,ShowHome);
        }
    }
}
