using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

public class Advertising : MonoBehaviour, IRewardedVideoAdListener
{
    public System.Action onVideoClosed;

    public static int countGameSession = 0;

    private void Start()
    {
        Appodeal.setAutoCache(Appodeal.REWARDED_VIDEO, false);
        Appodeal.initialize("9cb0c515be3554c7fe8f2cb02e5b54e554d20c1865ec0868", Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO, false);
        Appodeal.setRewardedVideoCallbacks(this);
    }

    public static void ShowVideoAd()
    {
        countGameSession++;
        if (countGameSession >= 3)
        {
            if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
            {
                Appodeal.show(Appodeal.REWARDED_VIDEO);
            }else if (Appodeal.isLoaded(Appodeal.INTERSTITIAL))
            {
                Appodeal.show(Appodeal.INTERSTITIAL);
            }
            Appodeal.cache(Appodeal.REWARDED_VIDEO);

            countGameSession = 1;
        }
    }

    public void onRewardedVideoLoaded(bool precache)
    {
        
    }

    public void onRewardedVideoFailedToLoad()
    {
        
    }

    public void onRewardedVideoShowFailed()
    {
        
    }

    public void onRewardedVideoShown()
    {
        
    }

    public void onRewardedVideoFinished(double amount, string name)
    {
        
    }

    public void onRewardedVideoClosed(bool finished)
    {
        onVideoClosed?.Invoke();
    }

    public void onRewardedVideoExpired()
    {
        
    }

    public void onRewardedVideoClicked()
    {
        
    }
}
