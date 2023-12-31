﻿using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;

namespace Brio.Game.World;

internal class TimeService : IDisposable
{
    public bool IsTimeFrozen
    {
        get => _updateEorzeaTimeHook.IsEnabled;
        set
        {

            if (value != IsTimeFrozen)
            {
                if (value)
                {
                    _updateEorzeaTimeHook.Enable();
                }
                else
                {
                    _updateEorzeaTimeHook.Disable();
                }
            }
        }
    }

    public unsafe long EorzeaTime
    {
        get
        {
            var framework = Framework.Instance();
            if (framework == null) return 0;
            return framework->IsEorzeaTimeOverridden ? framework->EorzeaTimeOverride : framework->EorzeaTime;
        }

        set
        {
            var framework = Framework.Instance();
            if (framework == null) return;
            framework->EorzeaTime = value;
            if (framework->IsEorzeaTimeOverridden)
                framework->EorzeaTimeOverride = value;
        }
    }

    public int MinuteOfDay
    {
        get
        {
            long currentTime = EorzeaTime;
            long timeVal = currentTime % 2764800;
            long secondInDay = timeVal % 86400;
            int minuteOfDay = (int)(secondInDay / 60f);
            return minuteOfDay;
        }

        set
        {
            EorzeaTime = value * 60 + 86400 * ((byte)DayOfMonth - 1);
        }
    }

    public int DayOfMonth
    {
        get
        {
            long currentTime = EorzeaTime;
            long timeVal = currentTime % 2764800;
            int dayOfMonth = (int)(Math.Floor(timeVal / 86400f) + 1);
            return dayOfMonth;
        }

        set
        {
            EorzeaTime = MinuteOfDay * 60 + 86400 * ((byte)value - 1);
        }
    }

    private delegate void UpdateEorzeaTimeDelegate(IntPtr a1, IntPtr a2);
    private readonly Hook<UpdateEorzeaTimeDelegate> _updateEorzeaTimeHook = null!;

    public TimeService(ISigScanner scanner, IGameInteropProvider hooking)
    {
        var etAddress = scanner.ScanText("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B F9 48 8B DA 48 81 C1 ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C");
        _updateEorzeaTimeHook = hooking.HookFromAddress<UpdateEorzeaTimeDelegate>(etAddress, UpdateEorzeaTime);
    }

    public void Dispose()
    {
        _updateEorzeaTimeHook?.Dispose();
    }

    internal void UpdateEorzeaTime(IntPtr a1, IntPtr a2)
    {
        // DO NOTHING
        // UpdateEorzeaTimeHook.Original(a1, a2);
    }
}
