using UnityEditor;
using UnityEngine;


/**********************************************************************
 * 
 *                      Stylesheet
 *      This static class provides some frequently used styles
 *      it requires to be initialized first. Multiple initializations 
 *      have no influence so in case of uncertain first call, call the 
 *      initstyles in both.
 * 
 **********************************************************************/

internal static class Stylesheet {
    // all different style classes
    public static GUIStyle headingStyle, footingStyle;
    public static GUIStyle selectingStyle;
    public static GUIStyle defaultStyle, titleStyle, subtitleStyle, redTextStyle;
    public static GUIStyle horizontalLine;

    // create styles here
    public static void InitStyles() {

        // text based
        defaultStyle = new GUIStyle();
        defaultStyle.fontSize = 12;

        titleStyle = new GUIStyle();
        titleStyle.fontSize = 25;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        subtitleStyle = new GUIStyle();
        subtitleStyle.fontSize = 20;
        subtitleStyle.normal.textColor = Color.white;
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.padding = new RectOffset(5, 5, 10, 10);

        redTextStyle = new GUIStyle();

        redTextStyle.fontStyle = FontStyle.Bold;
        redTextStyle.normal.textColor = Color.red;
        redTextStyle.alignment = TextAnchor.MiddleCenter;

        // structural based
        headingStyle = new GUIStyle();
        headingStyle.margin = new RectOffset(20, 20, 20, 20);

        footingStyle = new GUIStyle();
        footingStyle.margin = new RectOffset(10, 10, 10, 10);
        footingStyle.alignment = TextAnchor.MiddleRight;

        // styled buttons
        selectingStyle = new GUIStyle();
        selectingStyle.fontSize = 15;
        selectingStyle.padding = new RectOffset(5, 5, 2, 2);
        selectingStyle.normal.textColor = Color.white;
        selectingStyle.hover.textColor = Color.green;
        selectingStyle.onHover.textColor = Color.green;


        // other
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(10, 10, 4, 4);
        horizontalLine.fixedHeight = 2;

    }
}
