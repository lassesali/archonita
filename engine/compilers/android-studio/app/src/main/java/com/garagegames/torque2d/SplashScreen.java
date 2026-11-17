package com.garagegames.torque2d;

import java.io.IOException;
import java.io.InputStream;

import android.app.Activity;
import android.app.Dialog;
import android.content.Context;
import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.view.Window;
import android.view.WindowManager;
import android.widget.ImageView;
import android.widget.ImageView.ScaleType;

public class SplashScreen extends Dialog {
	
	public SplashScreen(Context context, int theme) {
		super(context, theme);
	}

	private static final Object _lock = new Object();
	private static SplashScreen _instance = null;
	private static boolean _shouldShow = false;
	
	public static void ToggleSplashScreen(final Context context, final boolean show, final int screenWidth, final int screenHeight) 
	{
		if (!(context instanceof Activity))
			return;

		final Activity activity = (Activity)context;

		synchronized (_lock)
		{
			_shouldShow = show;
		}

		activity.runOnUiThread(new Runnable() {
			@Override
			public void run() {
				synchronized (_lock)
				{
					if (_shouldShow)
					{
						if (activity.isFinishing())
							return;
						if (android.os.Build.VERSION.SDK_INT >= 17 && activity.isDestroyed())
							return;
						if (_instance != null && _instance.isShowing())
							return;

						InputStream stream = null;
						try {
							AssetManager assetMgr = context.getAssets();
							stream = assetMgr.open("splash.png");
							Bitmap splashBitmap = BitmapFactory.decodeStream(stream);
							if (splashBitmap == null)
								return;

							SplashScreen dialog = new SplashScreen(activity, android.R.style.Theme_Black_NoTitleBar_Fullscreen);
							dialog.requestWindowFeature(Window.FEATURE_NO_TITLE);
							dialog.setCancelable(false);

							ImageView view = new ImageView(activity);
							view.setPadding(0, 0, 0, 0);
							view.setImageBitmap(splashBitmap);
							view.setScaleType(ScaleType.FIT_CENTER);
							view.setBackgroundColor(Color.BLACK);
							dialog.setContentView(view);

							Window window = dialog.getWindow();
							if (window != null)
							{
								// Keep the dialog non-focusable and non-touchable so MyNativeActivity
								// never receives APP_CMD_LOST_FOCUS (which pauses engine->animating)
								// and does not surface the navigation bar over immersive mode.
								window.addFlags(
									WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
									| WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE
									| WindowManager.LayoutParams.FLAG_ALT_FOCUSABLE_IM
									| WindowManager.LayoutParams.FLAG_FULLSCREEN
									| WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN);
								window.setLayout(
									WindowManager.LayoutParams.MATCH_PARENT,
									WindowManager.LayoutParams.MATCH_PARENT);
								if (android.os.Build.VERSION.SDK_INT >= 19)
								{
									window.getDecorView().setSystemUiVisibility(
										android.view.View.SYSTEM_UI_FLAG_LAYOUT_STABLE
										| android.view.View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
										| android.view.View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
										| android.view.View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
										| android.view.View.SYSTEM_UI_FLAG_FULLSCREEN
										| android.view.View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
								}
							}

							dialog.show();
							_instance = dialog;
						} catch (IOException e) {
							android.util.Log.w("Torque2D", "Failed to load splash.png", e);
						} finally {
							if (stream != null)
							{
								try {
									stream.close();
								} catch (IOException ignored) {
								}
							}
						}
					}
					else if (_instance != null)
					{
						try {
							if (_instance.isShowing())
								_instance.dismiss();
						} catch (IllegalArgumentException ignored) {
						} finally {
							_instance = null;
						}
					}
				}
			}
		});
	}
}
