using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

public class Main : 
    MonoBehaviour, 
    PxPre.UIDock.IDockListener
{
    // https://web.archive.org/web/20160116150939/http://freespace.virgin.net/hugo.elias/graphics/x_water.htm
    //public Texture2D tex;
    //public Texture2D tex2;

    public enum ManipMode
    { 
        Translate,
        Rotate,
        Drag,
        Scale
    }

    [System.Serializable]
    public struct DemoFile
    { 
        public string name;

        public TextAsset asset;
        public string resourceName;
    }

    int dim = 1024;

    public float divAmt = 50.0f;

    public UnityEngine.UI.RawImage img;
    public RectTransform simViewport;
    PxPre.UIDock.Window simWindow;
    public UnityEngine.UI.ScrollRect simScrollRect;

    public Pane_Properties paneProperties;
    PxPre.UIDock.Window winProperties;

    public Pane_Info paneInfo;
    PxPre.UIDock.Window winInfo;

    public Material viewMatRedGreen;
    public Material viewMatGrayscale;
    public Material viewMatSpectrum;
    Material activeViewMat = null;

    float zoom = 1.0f;

    public float modulateRate = 5.0f;

    public PxPre.UIDock.Root dockSys;

    public RectTransform pulldownSim;
    public RectTransform pulldownView;
    public RectTransform pulldownHelp;

    public Canvas canvas;
    public PxPre.DropMenu.Props dockProp;

    public GameObject zoomControls;
    public UnityEngine.UI.Slider zoomSlider;

    public Sprite playIcon;
    public Sprite pauseIcon;
    public UnityEngine.UI.Image imgPlayPauseIcon;

    public WaveScene waveScene;
    public WaveSimulation waveSim;
    
    public PxPre.UIL.Factory uiFactory;

    ManipMode manipMode = ManipMode.Translate;
    public UnityEngine.UI.Image manipTransIcon;
    public UnityEngine.UI.Image manipRotIcon;
    public UnityEngine.UI.Image manipDragIcon;
    public UnityEngine.UI.Image manipScaleIcon;

    HashSet<SceneActor> dirtyActors = new HashSet<SceneActor>();

    public bool playing = true;

    SceneActor selection;
    public SceneActor Selected {get=>this.selection; }

    public List<DemoFile> demoScenarios = new List<DemoFile>();

    internal string documentComment = "";

    internal EditValue evDecay = 
        new EditValue(
            "Decay", 
            new ValFloat(Mathf.InverseLerp(MinDecay, MaxDecay, TargetRealDecay)), 
            new ValFloat(0.0f), 
            new ValFloat(1.0f), 
            new ValFloat(0.001f));

    const float MinDecay = 0.98f;
    const float MaxDecay = 1.0f;
    const float TargetRealDecay = 0.9999f;

    private void Awake()
    {
        this.waveSim = new WaveSimulation();
        this.waveSim.AllocateBuffers(dim);

        this.waveScene.Initialize(this.waveSim);
        this.dockSys.listener = this;

        Application.targetFrameRate = 30;
    }

    void Start()
    {
        this.SetViewMaterial(this.viewMatRedGreen);

        this.simWindow = 
            this.dockSys.WrapIntoWindow(
                this.simViewport, 
                "Simulation", 
                PxPre.UIDock.Window.Flag.LockedFrame);

        this.dockSys.DockWindow(this.simWindow, null, PxPre.UIDock.Root.DropType.Into);

        this.winProperties = 
            this.dockSys.WrapIntoWindow(
                this.paneProperties.rectTransform,
                "Properties");

        this.dockSys.DockWindow(this.winProperties, null, PxPre.UIDock.Root.DropType.Left);

        this.winInfo  = 
            this.dockSys.WrapIntoWindow(
                this.paneInfo.rectTransform,
                "Info");

        this.dockSys.DockWindowAtWin(
            this.winInfo, 
            this.winProperties, 
            PxPre.UIDock.Root.DropType.Bottom);


        this.zoomSlider.value = 1.0f;

        this.RefershManipIcons();

        foreach (Pane_Base pb in this.Panes())
            pb.Init(this);

        this.dockSys.ForceLayout();
        this.dockSys.ResizeAddressWithProportions(null, new float [] {1.0f, 2.0f });
        this.dockSys.ResizeAddressWithProportions(new int[]{0}, new float[] { 3.0f, 1.0f });

        this.SetZoom(0.0f, true);
        this.RefreshDecay();
    }

    void Update()
    {
        // We allow them to change actor properties and preview them while paused.
        foreach (SceneActor sa in this.dirtyActors)
            sa.UpdateGeometry(this.waveScene);

        if(this.playing == true)
            this.Integrate();
        else
            this.waveScene.UpdateBuffersWithoutIntegration();


        this.dirtyActors.Clear();

#if UNITY_EDITOR
        if(Input.GetKeyDown( KeyCode.S) == true)
        { 
            System.Xml.XmlDocument savedDoc = this.waveScene.Save("Comment");

            System.IO.StringWriter string_writer = new System.IO.StringWriter();
            System.Xml.XmlTextWriter xml_text_writer = new System.Xml.XmlTextWriter(string_writer);
            xml_text_writer.Formatting = System.Xml.Formatting.Indented;
            savedDoc.WriteTo(xml_text_writer);
            System.IO.File.WriteAllText("Saved.xml", string_writer.ToString());
        }

        if (Input.GetKeyDown(KeyCode.L) == true)
        {
            string str = System.IO.File.ReadAllText("Saved.xml");
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(str);

            this.LoadXML(doc);
        }
#endif

    }

    public void LoadDemoFile(DemoFile df)
    {
        TextAsset asset = df.asset;
        if(asset == null)
            asset = Resources.Load<TextAsset>(df.resourceName);

        if(asset == null)
            return;

        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        doc.LoadXml(asset.text);
        
        this.LoadXML(doc);
    }

    public void LoadXML(System.Xml.XmlDocument doc)
    {
        this.Clear();

        this.waveScene.Load(doc, false, out this.documentComment);

        this.NotifyLoad();
    }

    void Clear()
    { 
        this.documentComment = string.Empty;
        this.selection = null;

        this.waveScene.Clear();
        this.waveSim.ClearBuffers();
        this.NotifyClear();
    }

    public void Integrate()
    {
        this.waveScene.Integrate();
        this.img.texture = this.waveSim.SignalRecent;
    }

    public void OnSlider_Zoom()
    { 
        this.SetZoom(Mathf.Lerp(-1.0f, 1.0f, this.zoomSlider.value), false);
    }

    public void OnButton_ZoomIn()
    { 
        this.zoomSlider.value += 0.2f;
    }

    public void OnButton_ZoomOut()
    {
        this.zoomSlider.value -= 0.2f;
    }

    public void SetZoom(float newZoom, bool setScrollbar)
    { 
        this.zoom = Mathf.Clamp(newZoom, -1.0f, 1.0f);

        float realZoom = 1.0f;
        if(this.zoom > 0.0f)
            realZoom = Mathf.Lerp(1.0f, 2.0f, this.zoom);
        else if(this.zoom < 0.0f)
            realZoom = Mathf.Lerp(1.0f, 0.5f, -this.zoom);


        this.img.rectTransform.localScale =
            new Vector3(realZoom, realZoom, realZoom);

        if(setScrollbar == true)
        { 
            this.zoomSlider.value = newZoom * 0.5f + 0.5f;
        }
    }

    public void OnButton_PlayPause()
    { 
        this.playing = !this.playing;

        this.imgPlayPauseIcon.sprite = 
            this.playing ? 
                this.pauseIcon :
                this.playIcon;

        this.SetViewMaterial(this.activeViewMat);

        // There's something weird going on with the RawTexture where it doesn't initially
        // update the input material the first pause until it's given a poke.
        this.img.gameObject.SetActive(false);
        this.img.gameObject.SetActive(true);
        //this.activeViewMat.SetFloat("_ShowInput", this.playing ? 0.0f : 1.0f);
    }

    public void OnButton_Add()
    { 
    }

    public void OnPulldown_Sim()
    {
        PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();
        stk.AddAction("Clear Signal", ()=>{ this.OnMenu_ClearSignal(); });
        stk.PushMenu("Scenarios");

        foreach(DemoFile df in this.demoScenarios)
        { 
            DemoFile dfCpy = df;
            stk.AddAction(dfCpy.name, ()=>{ this.LoadDemoFile(dfCpy); });
        }
        stk.PopMenu();

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            this.canvas,
            stk.Root,
            this.pulldownSim);
    }

    public void OnPulldown_View()
    {
        PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();
        stk.AddAction(this.activeViewMat == this.viewMatRedGreen, null,     "Red/Green",    () => { this.OnMenu_SetRedGreen(); });
        stk.AddAction(this.activeViewMat == this.viewMatGrayscale, null,    "Greyscale",    () => { this.OnMenu_SetGreyscale(); });
        stk.AddAction(this.activeViewMat == this.viewMatSpectrum, null,     "Spectrum",     () => { this.OnMenu_SetSpectrum(); });
        stk.AddSeparator();
        stk.AddAction(this.winProperties != null, null, "Properties", () => { this.OnMenu_Properties(); });
        stk.AddAction(this.winInfo != null, null, "Info", () => { this.OnMenu_Info(); });
        stk.PopMenu();

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            this.canvas,
            stk.Root,
            this.pulldownView);
    }

    public void OnPulldown_Help()
    {
        PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();
        stk.AddAction("About", () => { });
        stk.PopMenu();

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            this.canvas,
            stk.Root,
            this.pulldownHelp);
    }

    void OnMenu_SetRedGreen()
    {
        SetViewMaterial(this.viewMatRedGreen);
    }

    void OnMenu_SetGreyscale()
    {
        SetViewMaterial(this.viewMatGrayscale);
    }

    void OnMenu_SetSpectrum()
    {
        SetViewMaterial(this.viewMatSpectrum);
    }

    void SetViewMaterial(Material mat)
    {
        this.activeViewMat = mat;
        this.activeViewMat.SetTexture("_Obs", this.waveSim.SimObstacles);
        this.activeViewMat.SetTexture("_Input", this.waveSim.InputTarget);
        this.activeViewMat.SetFloat("_ShowInput", this.playing ? 0.0f : 1.0f);
        this.img.material = this.activeViewMat;
    }

    void OnMenu_Properties()
    {
        if(this.winProperties != null)
            this.dockSys.CloseWindow(this.winProperties);
        else
        {
            this.paneProperties.gameObject.SetActive(true);
            this.winProperties = this.dockSys.WrapIntoWindow(this.paneProperties.rectTransform, "Properties");
            this.dockSys.DockWindow(this.winProperties, null, PxPre.UIDock.Root.DropType.Left);
        } 
    }

    void OnMenu_Info()
    {
        if (this.winInfo != null)
            this.dockSys.CloseWindow(this.winInfo);
        else
        {
            this.paneInfo.gameObject.SetActive(true);
            this.winInfo = this.dockSys.WrapIntoWindow(this.paneInfo.rectTransform, "Info");
            this.dockSys.DockWindow(this.winInfo, null, PxPre.UIDock.Root.DropType.Left);
        }
    }

    void OnMenu_ClearSignal()
    { 
        this.waveSim.ClearBuffers();
    }


    public Ray ? GetRayAtUIMouse(Vector2 mouse)
    {
        Vector2 rectRelPt =
            this.simScrollRect.transform.worldToLocalMatrix.MultiplyPoint(mouse);

        if (this.simScrollRect.viewport.rect.Contains(rectRelPt) == false)
            return null;


        Vector3 local = this.img.transform.worldToLocalMatrix.MultiplyPoint(mouse);
        Rect r = this.img.rectTransform.rect;
        Vector3 vp =
            new Vector3(
                Mathf.InverseLerp(r.xMin, r.xMax, local.x),
                Mathf.InverseLerp(r.yMin, r.yMax, local.y),
                0.0f);

        Ray ray = this.waveScene.simObstacleCam.ViewportPointToRay(vp);
        return ray;
    }


    void CreateAddMenu(Vector2 createPos, PxPre.DropMenu.StackUtil stk)
    {
        const float obsRad = 0.5f;
        stk.PushMenu("Create Barrier");
        stk.AddAction("Box", () => { this.CreateShape(createPos, SceneActor.Type.Barrier, SceneActor.Shape.Square, SceneActor.Fill.Filled, obsRad, obsRad); });
        stk.AddAction("Circle", () => { this.CreateShape(createPos, SceneActor.Type.Barrier, SceneActor.Shape.Ellipse, SceneActor.Fill.Filled, obsRad, obsRad); });
        stk.AddAction("Beam", () => { this.CreateShape(createPos, SceneActor.Type.Barrier, SceneActor.Shape.Square, SceneActor.Fill.Filled, obsRad * 2, obsRad / 10.0f); });
        stk.AddAction("Dish", () => { this.CreateShape(createPos, SceneActor.Type.Barrier, SceneActor.Shape.Ellipse, SceneActor.Fill.Hollow, obsRad * 2.0f, obsRad * 2.0f, -45.0f, 45.0f); });
        stk.AddAction("Pin", () => { this.CreateShape(createPos, SceneActor.Type.Barrier, SceneActor.Shape.Ellipse, SceneActor.Fill.Filled, 0.05f, 0.05f); });
        stk.PopMenu();
        //
        stk.PushMenu("Create Emitter");
        stk.AddAction("Pin Emitter", () => { this.CreateShape(createPos, SceneActor.Type.Emitter, SceneActor.Shape.Ellipse, SceneActor.Fill.Filled, 0.05f, 0.05f); });
        stk.AddAction("Beam Emitter", () => { this.CreateShape(createPos, SceneActor.Type.Emitter, SceneActor.Shape.Square, SceneActor.Fill.Filled, obsRad * 2, obsRad / 10.0f); });
        stk.PopMenu();
    }

    public IEnumerable<Pane_Base> Panes()
    { 
        yield return this.paneInfo;
        yield return this.paneProperties;
    }

    public void AddActor(SceneActor actor)
    { 
        if(this.waveScene.AddActor(actor) == true)
            this.NotifyActorAdded(actor);

        actor.UpdateGeometry(this.waveScene);
    }

    public void RemoveActor(SceneActor actor)
    {
        if(this.waveScene.DestroyActor(actor) == false)
            return;

        this.NotifyActorRemoved(actor);

    }

    protected void NotifyClear()
    {
        foreach (Pane_Base pb in this.Panes())
            pb.OnCleared();
    }

    protected void NotifyLoad()
    {
        foreach (Pane_Base pb in this.Panes())
            pb.OnLoaded();
    }

    protected void NotifyActorAdded(SceneActor actor)
    {
        foreach (Pane_Base pb in this.Panes())
            pb.OnActorAdded(actor);
    }

    protected void NotifyActorRemoved(SceneActor actor)
    {
        if(this.selection == actor)
            this.selection = null;

        foreach (Pane_Base pb in this.Panes())
            pb.OnActorDeleted(actor);

        this.dirtyActors.Remove(actor);
    }

    public void NotifyActorModified(SceneActor actor, EditValue ev)
    { 
        this.NotifyActorModified(actor, ev.name);
    }

    public void NotifyActorModified(SceneActor actor, string name)
    { 
        if(actor == null)
        {
            if(name == "Decay")
                this.RefreshDecay();
        }

        foreach(Pane_Base pb in this.Panes())
            pb.OnActorModified(actor, name);

        if(actor != null)
            this.dirtyActors.Add(actor);
    }

    public void RefreshDecay()
    { 
        this.waveScene.simMaterial.SetFloat("_Decay", Mathf.Lerp( MinDecay, MaxDecay, this.evDecay.FloatVal));
    }

    protected void NotifyActorSelected(SceneActor actor)
    {
        foreach (Pane_Base pb in this.Panes())
            pb.OnActorSelected(actor);
    }

    public void SelectActor(SceneActor actor)
    { 
        if(this.selection == actor)
            return;

        this.selection = actor;
        NotifyActorSelected(this.selection);
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      INTERFACE : IDockListener
    //
    ////////////////////////////////////////////////////////////////////////////////

    bool PxPre.UIDock.IDockListener.RequestUndock(PxPre.UIDock.Root r, PxPre.UIDock.Window win)
    {  return true; }

    bool PxPre.UIDock.IDockListener.RequestClose(PxPre.UIDock.Root r, PxPre.UIDock.Window win)
    { 
        if(win == this.winInfo || win == this.winProperties)
        { 
            win.Win.SetParent(null, false);
            win.Win.gameObject.SetActive(false);

            if(win == this.winInfo)
                this.winInfo = null;
            else if(win == this.winProperties)
                this.winProperties = null;
        }

        return true; 
    }

    void PxPre.UIDock.IDockListener.OnClosing(PxPre.UIDock.Root r, PxPre.UIDock.Window win)
    {}

    public void CreateShape(Vector2 pos, SceneActor.Type type, SceneActor.Shape shape, SceneActor.Fill fill, float radius1, float radius2, float angle1 = -180.0f, float angle2 = 180.0f)
    { 
        SceneActor act = new SceneActor();

        act.posx.val.SetFloat(pos.x);
        act.posy.val.SetFloat(pos.y);

        act.rot.val.SetFloat(0.0f);

        act.radius1.FloatVal = radius1;
        act.radius2.FloatVal = radius2;

        act.squared.BoolVal = (radius1 == radius2);

        act.angle1.FloatVal = angle1;
        act.angle2.FloatVal = angle2;

        act.shape.val.SetInt((int)shape);
        act.actorType.val.SetInt((int)type);
        act.fillMode.val.SetInt((int)fill);

        this.AddActor(act);
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      HANDLER : SimEventDirect
    //
    ////////////////////////////////////////////////////////////////////////////////
    
    Vector2 originalSimDragClick = Vector2.zero;
    Vector2 origDragRadii = Vector2.one;
    SceneActor draggedActor = null;

    public void OnSimEvent_OnBeginDrag(UnityEngine.EventSystems.PointerEventData ped)
    { 
    }

    public void OnSimEvent_OnEndDrag(UnityEngine.EventSystems.PointerEventData ped)
    {
        this.draggedActor = null;
    }

    public void OnSimEvent_OnDrag(UnityEngine.EventSystems.PointerEventData ped)
    {
        if(this.draggedActor != null)
        {
            Ray? r = GetRayAtUIMouse(ped.position);
            if(r.HasValue)
            {
                Vector3 onObj = this.draggedActor.gameObject.transform.localToWorldMatrix.MultiplyPoint(this.originalSimDragClick);

                if (this.manipMode == ManipMode.Translate)
                {
                    Vector3 pos = this.draggedActor.gameObject.transform.position + (r.Value.origin - onObj);
                    pos.z = 0.0f;
                    this.draggedActor.gameObject.transform.position = pos;

                    this.draggedActor.posx.FloatVal = pos.x;
                    this.draggedActor.posy.FloatVal = pos.y;
                    this.NotifyActorModified(draggedActor, "X");
                    this.NotifyActorModified(draggedActor, "Y");
                }
                else if(this.manipMode == ManipMode.Rotate)
                { 
                    Vector2 curDragVec = this.draggedActor.gameObject.transform.localToWorldMatrix.MultiplyVector(this.originalSimDragClick);
                    Vector2 targDragVec = r.Value.origin - this.draggedActor.gameObject.transform.position;
                    float rotAmt = Vector2.SignedAngle(curDragVec, targDragVec);
                    this.draggedActor.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotAmt) * this.draggedActor.gameObject.transform.rotation;

                    float zr = this.draggedActor.gameObject.transform.rotation.eulerAngles.z;
                    this.draggedActor.rot.FloatVal = ((zr + 180.0f) % 360.0f + 360.0f) % 360.0f - 180.0f;
                    this.NotifyActorModified(draggedActor, "Rotation");
                }
                else if(this.manipMode == ManipMode.Drag)
                {
                    float mag = this.originalSimDragClick.magnitude;
                    Vector2 curDragVec = this.draggedActor.gameObject.transform.localToWorldMatrix.MultiplyVector(this.originalSimDragClick);
                    Vector2 targVec = r.Value.origin - this.draggedActor.gameObject.transform.position;
                    float rotAmt = Vector2.SignedAngle(curDragVec, targVec);
                    this.draggedActor.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotAmt) * this.draggedActor.gameObject.transform.rotation;

                    Vector2 targPos = (Vector2)r.Value.origin - targVec.normalized * mag;
                    this.draggedActor.gameObject.transform.position = targPos;

                    float zr = this.draggedActor.gameObject.transform.rotation.eulerAngles.z;
                    this.draggedActor.rot.FloatVal = ((zr + 180.0f) % 360.0f + 360.0f) % 360.0f - 180.0f;
                    this.NotifyActorModified(draggedActor, "Rotation");

                    this.draggedActor.posx.FloatVal = targPos.x;
                    this.draggedActor.posy.FloatVal = targPos.y;
                    this.NotifyActorModified(draggedActor, "X");
                    this.NotifyActorModified(draggedActor, "Y");
                }
                else if(this.manipMode == ManipMode.Scale)
                {
                    Vector2 newLocal = this.draggedActor.gameObject.transform.worldToLocalMatrix.MultiplyPoint(r.Value.origin);

                    float x = this.origDragRadii.x;
                    float y = this.origDragRadii.y;

                    if(Mathf.Abs(this.originalSimDragClick.x) > 0.0f)
                        x *= Mathf.Abs(newLocal.x / this.originalSimDragClick.x);

                    if (Mathf.Abs(this.originalSimDragClick.y) > 0.0f)
                        y *= Mathf.Abs(newLocal.y / this.originalSimDragClick.y);

                    if(draggedActor.squared.BoolVal == true)
                        y = x;

                    this.draggedActor.radius1.FloatVal = x;
                    this.draggedActor.radius2.FloatVal = y;
                    this.NotifyActorModified(draggedActor, draggedActor.radius1);
                    this.NotifyActorModified(draggedActor, draggedActor.radius2);
                }
            }

        }
        else
        {
            Vector2 cp =
                this.img.rectTransform.worldToLocalMatrix.MultiplyPoint(ped.position);

            Vector2 diff = this.originalSimDragClick - cp;

            Rect rimg = this.img.rectTransform.rect;

            diff.x /= rimg.width;
            diff.y /= rimg.height;

            Vector2 newNormPos = this.simScrollRect.normalizedPosition + diff;
            newNormPos.x = Mathf.Clamp01(newNormPos.x);
            newNormPos.y = Mathf.Clamp01(newNormPos.y);

            this.simScrollRect.normalizedPosition = newNormPos;
        }
    }

    public void OnSimEvent_OnPointerDown(UnityEngine.EventSystems.PointerEventData ped)
    {
        this.draggedActor = null;

        Ray ? r = GetRayAtUIMouse(ped.position);
        SceneActorTag sat = null;
        if(r.HasValue == true)
        { 
            RaycastHit [] rhs = Physics.RaycastAll(r.Value);
            foreach(RaycastHit rh in rhs)
            { 
                sat = rh.collider.gameObject.GetComponent<SceneActorTag>();
                break;
            }
        }

        if(ped.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
        {
            if (r.HasValue == true)
            {

                PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();

                stk.AddAction("Clear Signal", ()=>{ this.OnMenu_ClearSignal(); });
                stk.AddSeparator();

                if(sat != null)
                { 
                    stk.PushMenu("Set Shape");
                        stk.AddAction(
                            sat.actor.shape.IntVal == (int)SceneActor.Shape.Ellipse,
                            null,
                            "Ellipse", 
                            ()=>
                            { 
                                sat.actor.shape.IntVal = (int)SceneActor.Shape.Ellipse; 
                                this.NotifyActorModified(sat.actor, sat.actor.shape.name);
                            });
                        stk.AddAction(
                            sat.actor.shape.IntVal == (int)SceneActor.Shape.Square,
                            null,
                            "Rectangle", 
                            ()=> 
                            { 
                                sat.actor.shape.IntVal = (int)SceneActor.Shape.Square;
                                this.NotifyActorModified(sat.actor, sat.actor.shape.name);
                            });
                    stk.PopMenu();

                    stk.PushMenu("Set Fill");
                        stk.AddAction(
                            sat.actor.fillMode.IntVal == (int)SceneActor.Fill.Hollow,
                            null,
                            "Hollow", 
                            () => 
                            {
                                sat.actor.fillMode.IntVal = (int)SceneActor.Fill.Hollow;
                                this.NotifyActorModified(sat.actor, sat.actor.fillMode.name);
                            });
                        stk.AddAction(
                            sat.actor.fillMode.IntVal == (int)SceneActor.Fill.Filled,
                            null,
                            "Fill", 
                            () => 
                            {
                                sat.actor.fillMode.IntVal = (int)SceneActor.Fill.Filled;
                                this.NotifyActorModified(sat.actor, sat.actor.fillMode.name);
                            });
                    stk.PopMenu();

                    stk.PushMenu("Set Type");
                        stk.AddAction(
                            sat.actor.actorType.IntVal == (int)SceneActor.Type.Emitter,
                            null,
                            "Emitter", 
                            () => 
                            {
                                sat.actor.actorType.IntVal = (int)SceneActor.Type.Emitter;
                                this.NotifyActorModified(sat.actor, sat.actor.actorType.name);
                            });
                        stk.AddAction(
                            sat.actor.actorType.IntVal == (int)SceneActor.Type.Barrier,
                            null,
                            "Barrier", 
                            () => 
                            {
                                sat.actor.actorType.IntVal = (int)SceneActor.Type.Barrier;
                                this.NotifyActorModified(sat.actor, sat.actor.actorType.name);
                            });
                        stk.AddAction(
                            sat.actor.actorType.IntVal == (int)SceneActor.Type.Impedance,
                            null,
                            "Impedance", 
                            () => 
                            {
                                sat.actor.actorType.IntVal = (int)SceneActor.Type.Impedance;
                                this.NotifyActorModified(sat.actor, sat.actor.actorType.name);
                            });
                        stk.AddAction(
                            sat.actor.actorType.IntVal == (int)SceneActor.Type.Sensor,
                            null,
                            "Sensor", 
                            () => 
                            {
                                sat.actor.actorType.IntVal = (int)SceneActor.Type.Sensor;
                                this.NotifyActorModified(sat.actor, sat.actor.actorType.name);
                            });
                    stk.PopMenu();

                    stk.AddSeparator();

                    stk.AddAction("Delete", ()=>{this.RemoveActor( sat.actor); });

                }
                else
                {
                    this.CreateAddMenu(r.Value.origin, stk);
                }


                PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
                    this.canvas,
                    stk.Root,
                    ped.position);

                return;
            }
        }

        if (sat != null)
        {
            draggedActor = sat.actor;
            this.SelectActor(draggedActor);
        }

        if (draggedActor != null)
        {
            this.origDragRadii = 
                new Vector2(
                    sat.actor.radius1.FloatVal, 
                    sat.actor.radius2.FloatVal);

            this.originalSimDragClick = 
                sat.transform.worldToLocalMatrix.MultiplyPoint(r.Value.origin);
        }
        else
        {
            this.originalSimDragClick = 
                this.img.rectTransform.worldToLocalMatrix.MultiplyPoint(ped.position);
        }
    }

    public void OnSimEvent_OnPointerUp(UnityEngine.EventSystems.PointerEventData ped)
    {
        this.draggedActor = null;
    }

    public void OnSimEvent_OnPointerEnter(UnityEngine.EventSystems.PointerEventData ped)
    {
    }

    public void OnSimEvent_OnPointerExit(UnityEngine.EventSystems.PointerEventData ped)
    {
    }

    public void OnSimEvent_OnScroll(UnityEngine.EventSystems.PointerEventData ped)
    {

        float uz = this.zoom + ped.scrollDelta.y * 0.05f;
        this.SetZoom(uz, true);
    }

    public void SkipSimSteps(int steps)
    { 
        for(int i = 0; i < steps; ++i)
            this.Integrate();
    }

    public void OnButton_SkipSteps1()
    {
        this.SkipSimSteps(1);
    }

    public void OnButton_SkipSteps10()
    { 
        this.SkipSimSteps(10);
    }

    public void OnButton_SkipSteps100()
    { 
        this.SkipSimSteps(100);
    }

    public void OnButton_ClearSignal()
    { 
        this.OnMenu_ClearSignal();
    }

    public void OnButton_ModeTranslate()
    {
        this.manipMode = ManipMode.Translate;
        this.RefershManipIcons();
    }

    public void OnButton_ModeRotation()
    {
        this.manipMode = ManipMode.Rotate;
        this.RefershManipIcons();
    }

    public void OnButton_ModeTranslateRotate()
    {
        this.manipMode = ManipMode.Drag;
        this.RefershManipIcons();
    }

    public void OnButton_ModeScale()
    {
        this.manipMode = ManipMode.Scale;
        this.RefershManipIcons();
    }

    public void RefershManipIcons()
    {
        float f = (this.manipMode == ManipMode.Translate) ? 1.0f : 0.2f;
        this.manipTransIcon.color = new Color(1.0f, 1.0f, 1.0f, f);

        f = (this.manipMode == ManipMode.Rotate) ? 1.0f : 0.2f;
        this.manipRotIcon.color = new Color(1.0f, 1.0f, 1.0f, f);

        f = (this.manipMode == ManipMode.Drag) ? 1.0f : 0.2f;
        this.manipDragIcon.color = new Color(1.0f, 1.0f, 1.0f, f);

        f = (this.manipMode == ManipMode.Scale) ? 1.0f : 0.2f;
        this.manipScaleIcon.color = new Color(1.0f, 1.0f, 1.0f, f);
    }
}
