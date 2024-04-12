import android.content.Context;
import android.content.DialogInterface;

import com.skydoves.colorpickerview.ColorEnvelope;
import com.skydoves.colorpickerview.ColorPickerDialog;
import com.skydoves.colorpickerview.listeners.ColorEnvelopeListener;

public class TestClass {
    public interface IColorPicker
    {
        void done(boolean res, int color);
    }
    
    public static void ColorPickerShow(Context context, String title, IColorPicker picker)
    {
        new ColorPickerDialog.Builder(context)
                .setTitle(title)
                .setPreferenceName("MyColorPickerDialog")
                .setPositiveButton("确定",
                        (ColorEnvelopeListener) (envelope, fromUser) ->
                                picker.done(true, envelope.getColor()))
                .setNegativeButton("取消",
                        (dialogInterface, i) -> dialogInterface.dismiss())
                .attachAlphaSlideBar(true) // the default value is true.
                .attachBrightnessSlideBar(true)  // the default value is true.
                .setBottomSpace(12) // set a bottom space between the last slidebar and buttons.
                .create();
    }
}
