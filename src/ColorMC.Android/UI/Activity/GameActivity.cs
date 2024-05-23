using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.GameButton;
using ColorMC.Android.GLRender;
using System;
using System.Linq;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace ColorMC.Android.UI.Activity;

[Activity(Label = "GameActivity",
    Theme = "@style/Theme.AppCompat.NoActionBar.FullScreen",
    TaskAffinity = "colormc.android.game.render",
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    Icon = "@drawable/icon")]
public class GameActivity : AppCompatActivity, IButtonFuntion
{
    private RelativeLayout _buttonList;
    private GLSurface view;
    private RelativeLayout buttonTool;
    private Button button1;
    private bool isEdit;
    private bool isMouse;
    private string nowLayout;
    private ButtonLayout _layout;
    private ButtonGroup _group;

    public bool IsEdit => isEdit;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        _buttonList = FindViewById<RelativeLayout>(Resource.Id.button_view)!;

        var display = AndroidHelper.GetDisplayMetrics(this);

        buttonTool = FindViewById<RelativeLayout>(Resource.Id.button_tool)!;
        button1 = FindViewById<Button>(Resource.Id.button1)!;
        button1.Click += Button1_Click;

        if (Intent?.GetBooleanExtra("EDIT_LAYOUT", false) == true)
        {
            button1.Visibility = ViewStates.Visible;
            isEdit = true;
            ShowLayoutList();
            return;
        }
        else if (Intent?.GetBooleanExtra("EDIT_GROUP", false) == true)
        {
            button1.Visibility = ViewStates.Visible;
            isEdit = true;
            ShowGroupList();
            return;
        }

        var uuid = Intent?.GetStringExtra("GAME_UUID");
        if (uuid != null
            && MainActivity.Games.TryGetValue(uuid, out var game))
        {
            view = new GLSurface(ApplicationContext, display);
            FindViewById<RelativeLayout>(Resource.Id.surface_view)!
                .AddView(view);

            game.GameClose = GameClose;
            view.SetGame(game);
        }
    }

    private void Button1_Click(object? sender, EventArgs e)
    {
        ShowLayoutList();
    }

    public void ShowLayoutList()
    {
        new ButtonLayoutListDialog(this);
    }

    public void ShowGroupList()
    {
        new ButtonGroupListDialog(this);
    }

    public void EditLayout(string name)
    {
        LoadLayout(name);

        buttonTool.Visibility = ViewStates.Visible;
    }

    public void EditGroup(string name)
    {

    }

    private void GameClose()
    {
        AndroidHelper.Main.Post(() =>
        {
            if (MainActivity.Games.Count == 0)
            {
                Finish();
            }
            else
            {
                view.SetGame(MainActivity.Games.Values.ToArray()[0]);
            }
        });
    }

    public override void OnBackPressed()
    {
        if (view?.NowGame?.IsClose == false)
        {
            _ = new AlertDialog.Builder(this)!
                .SetMessage(Resource.String.game_info1)!
                .SetCancelable(false)!
                .SetPositiveButton(Resource.String.game_info2, (a, b) =>
                {
                    view.NowGame.Kill();
                    Finish();
                })
                .SetNegativeButton(Resource.String.game_info3, (a, b) =>
                {
                    Finish();
                })
                .Show();
        }
        else
        {
            Finish();
        }
    }

    public void LoadGroup(ButtonGroup group)
    {
        _group = group;

        LoadLayout(group.MainLayout);
    }

    public void LoadLayout(string name)
    {
        nowLayout = name;
        if (!ButtonManage.ButtonLayouts.TryGetValue(name, out var layout))
        {
            return;
        }

        LoadLayout(layout);
    }

    public void LoadLayout(ButtonLayout layout)
    {
        _buttonList.RemoveAllViews();

        foreach (var item in layout.Buttons)
        {
            _buttonList.AddView(new ButtonView(item, this, this));
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        var uuid = intent?.GetStringExtra("GAME_UUID");
        if (uuid != null &&
            MainActivity.Games.TryGetValue(uuid, out var game))
        {
            game.GameClose = GameClose;
            view.SetGame(game);
        }
    }

    public void ShowSetting()
    {
        var dialogFragment = new TabsDialogFragment(view);
        dialogFragment.Show(SupportFragmentManager, "tabs_dialog");
    }

    public void NextGroup()
    {

    }

    public void LastGroup()
    {

    }

    public void GoEdit(ButtonData data)
    {
        new ButtonEditDialog(this, data);
    }
}