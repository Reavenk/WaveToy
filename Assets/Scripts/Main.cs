using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : 
    MonoBehaviour, 
    PxPre.UIDock.IDockListener
{
    // https://web.archive.org/web/20160116150939/http://freespace.virgin.net/hugo.elias/graphics/x_water.htm
    //public Texture2D tex;
    //public Texture2D tex2;

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

    public Material drawMat;

    public UnityEngine.UI.Image dragReticule;
    public UnityEngine.UI.Text dragNotice;
    

    public PxPre.UIL.Factory uiFactory;

    HashSet<SceneActor> dirtyActors = new HashSet<SceneActor>();

    public bool playing = true;

    private void Awake()
    {
        this.waveSim = new WaveSimulation();
        this.waveSim.AllocateBuffers(dim);

        this.waveScene.Initialize(this.waveSim);
        this.dockSys.listener = this;
    }

    void Start()
    {
        this.activeViewMat = this.viewMatRedGreen;
        this.img.material.SetTexture("_Obs", this.waveSim.SimObstacles);

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

        this.DisableAddNotices();

        foreach(Pane_Base pb in this.Panes())
            pb.Init(this);
    }

    void Update()
    {
        if(this.playing == true)
            this.Integrate();

        foreach(SceneActor sa in this.dirtyActors)
            sa.UpdateGeometry();

        this.dirtyActors.Clear();
    }

    public void Integrate()
    {
        this.waveScene.Integrate();

        this.img.texture = this.waveSim.SignalRecent;

        float f = Mathf.Sin(Time.time * Mathf.PI * 2 * this.modulateRate) * 0.5f;
        drawMat.SetColor("_Color", new Color(f, f, f, 1.0f));
    }

    void DisableAddNotices()
    {
        this.dragReticule.gameObject.SetActive(false);
        this.dragNotice.gameObject.SetActive(false);
    }

    void EnableAddDrag()
    {
        this.dragReticule.gameObject.SetActive(true);
        this.dragNotice.gameObject.SetActive(true);
        this.dragNotice.text = "Drop to Specify Creation Location";
    }

    void EnableAddClick()
    {
        this.dragReticule.gameObject.SetActive(false);
        this.dragNotice.gameObject.SetActive(true);
        this.dragNotice.text = "Select Creation Location.";
    }

    public void OnSlider_Zoom()
    { 
        this.SetZoom(Mathf.Lerp(-1.0f, 1.0f, this.zoomSlider.value));
    }

    public void OnButton_ZoomIn()
    { 
        this.zoomSlider.value += 0.2f;
    }

    public void OnButton_ZoomOut()
    {
        this.zoomSlider.value -= 0.2f;
    }

    public void SetZoom(float newZoom)
    { 
        this.zoom = Mathf.Clamp(newZoom, -1.0f, 1.0f);

        float realZoom = 1.0f;
        if(this.zoom > 0.0f)
            realZoom = Mathf.Lerp(1.0f, 2.0f, this.zoom);
        else if(this.zoom < 0.0f)
            realZoom = Mathf.Lerp(1.0f, 0.5f, -this.zoom);


        this.img.rectTransform.localScale =
            new Vector3(realZoom, realZoom, realZoom);
    }

    public void OnButton_PlayPause()
    { 
        this.playing = !this.playing;

        this.imgPlayPauseIcon.sprite = 
            this.playing ? 
                this.pauseIcon :
                this.playIcon;
    }

    public void OnButton_Add()
    { 
    }

    public void OnPulldown_Sim()
    {
        PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();
        stk.AddAction("Clear Signal", ()=>{ this.OnMenu_ClearSignal(); });
        stk.AddAction("Clear Scene", ()=>{ });
        stk.PushMenu("Scenarios");
            stk.AddAction("--", ()=>{ });
            stk.AddAction("--", () => { });
            stk.AddAction("--", () => { });
        stk.PopMenu();

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            this.canvas,
            stk.Root,
            this.pulldownSim);
    }

    public void OnPulldown_View()
    {
        PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();
        stk.AddAction(this.zoomControls.activeSelf, null, "Zoom", ()=>{ this.OnMenu_ToggleZoom(); });
        stk.AddSeparator();
        stk.AddAction(this.activeViewMat == this.viewMatRedGreen, null, "Red/Green", () => { this.OnMenu_SetRedGreen(); });
        stk.AddAction(this.activeViewMat == this.viewMatGrayscale, null, "Greyscale", () => { this.OnMenu_SetGreyscale(); });
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

    void OnMenu_ToggleZoom()
    { 
        this.zoomControls.SetActive( !this.zoomControls.activeSelf );
    }

    void OnMenu_SetRedGreen()
    { 
        this.activeViewMat = this.viewMatRedGreen;
        this.activeViewMat.SetTexture("_Obs", this.waveSim.SimObstacles);
        this.img.material = this.activeViewMat;
    }

    void OnMenu_SetGreyscale()
    { 
        this.activeViewMat = this.viewMatGrayscale;
        this.activeViewMat.SetTexture("_Obs", this.waveSim.SimObstacles);
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

    void OnMenu_ClearScene()
    { 
    }

    public void DefferedAddDrag_OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.EnableAddDrag();
    }

    public void DefferedAddDrag_OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.DisableAddNotices();


        Vector2 rectRelPt = 
            this.simScrollRect.transform.worldToLocalMatrix.MultiplyPoint(eventData.position);

        if(this.simScrollRect.viewport.rect.Contains(rectRelPt) == false)
            return;

        // TODO: Find point
        Vector2 insertionPt = rectRelPt;

        PxPre.DropMenu.StackUtil stk = new PxPre.DropMenu.StackUtil();
        stk.PushMenu("Create Barrier");
        stk.AddAction("Box", ()=>{ this.CreateShape(insertionPt, SceneActor.Type.Barrier, SceneActor.Shape.Square, SceneActor.Fill.Filled); });
        stk.AddAction("Cup", () => { this.CreateShape(insertionPt, SceneActor.Type.Barrier, SceneActor.Shape.Ellipse, SceneActor.Fill.Hollow); });
        stk.AddAction("Ellipse", () => { this.CreateShape(insertionPt, SceneActor.Type.Barrier, SceneActor.Shape.Ellipse, SceneActor.Fill.Filled); });
        stk.AddAction("Circle", () => { this.CreateShape(insertionPt, SceneActor.Type.Barrier, SceneActor.Shape.Ellipse, SceneActor.Fill.Filled); });
        stk.PopMenu();
        //
        stk.PushMenu("Create Emitter");
        stk.AddAction("Yo1", null);
        stk.PopMenu();
        //
        stk.PushMenu("Create Sensor");
        stk.AddAction("Yo1", null);
        stk.PopMenu();
        

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            this.canvas, 
            stk.Root, 
            eventData.position);
            
    }

    public void DefferedAddDrag_OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
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
    }

    public void RemoveActor(SceneActor actor)
    {
    }

    protected void NotifyActorAdded(SceneActor actor)
    {
        foreach (Pane_Base pb in this.Panes())
            pb.OnActorAdded(actor);
    }

    protected void NotifyActorRemoved(SceneActor actor)
    {
        foreach (Pane_Base pb in this.Panes())
            pb.OnActorDeleted(actor);

        this.dirtyActors.Remove(actor);
    }

    public void NotifyActorModified(SceneActor actor, string name)
    { 
        foreach(Pane_Base pb in this.Panes())
            pb.OnActorModified(actor, name);

        this.dirtyActors.Add(actor);
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

    public void CreateShape(Vector2 pos, SceneActor.Type type, SceneActor.Shape shape, SceneActor.Fill fill)
    { 
        SceneActor act = new SceneActor();
        act.posx.val.SetFloat(pos.x);
        act.posy.val.SetFloat(pos.y);
        act.rot.val.SetFloat(0.0f);

        this.AddActor(act);
    }
}
