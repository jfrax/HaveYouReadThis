# Changelog

## 5.1.1 — Fix stale party-progress text

- The item-description party progress (`GetPartyProgressString`) no longer lags or
  stays stale until you reselect the item. On 3.0, `XUiC_ItemInfoWindow.Update`
  overwrites its own `IsDirty` flag every frame (driven only by the Inspect/Shift
  key), so `RefreshAllWindows()` alone no longer rebuilds the description. We now
  force the visible item info window to rebuild directly via `SetItemStack`, which
  refreshes the text immediately on both host and client.

## 5.1.0 — 7 Days To Die 3.0 compatibility

Updated the mod to work against the 3.0 base game (experimental), which made
breaking changes to the persistent-player / ally system.

- Replaced the Harmony patch on the now-removed `GameManager.PersistentPlayerEvent`
  with a patch on `AllyStore.AllyUpdateResponse`, the new choke point for ally
  changes (fires on both server and client). Local crafting-icon overlays once
  again refresh when allies are added or removed.
- Rewrote the offline-ally lookup: `PersistentPlayerData.ACL` was removed in 3.0
  and relationships now live in `GameManager.persistentPlayers.Allies` (`AllyStore`).
  Ally status is now resolved via `AllyStore.EnumerateAllies`, so it still works
  for offline allies.

No gameplay or networking behavior changed — this is a compatibility update.

## 5.0.0

- Initial public release for 7 Days To Die 2.5.
