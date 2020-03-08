using UnityEngine;

namespace StlVault.Views
{
    public static class RendererExtensions
    {
        public static bool IsVisibleOn(this RectTransform elem, Camera cam)
        {
            //just use the screen as the viewport
            var viewportMinCorner = new Vector2( 0, 0 );
            var viewportMaxCorner = new Vector2( Screen.width, Screen.height);

            //give 1 pixel border to avoid numeric issues:
            viewportMinCorner += Vector2.one;
            viewportMaxCorner -= Vector2.one;
     
            //do a similar procedure, to get the "element's" corners relative to screen:
            var worldCorners = new Vector3[4];
            elem.GetWorldCorners(worldCorners);
     
            Vector2 elemMinCorner = worldCorners[0];
            Vector2 elemMaxCorner = worldCorners[2];
     
            //perform comparison:
            if(elemMinCorner.x > viewportMaxCorner.x) return false;
            if(elemMinCorner.y > viewportMaxCorner.y) return false;
            if(elemMaxCorner.x < viewportMinCorner.x) return false;
            if(elemMaxCorner.y < viewportMinCorner.y) return false;
         
            return true;
        }
    }
}