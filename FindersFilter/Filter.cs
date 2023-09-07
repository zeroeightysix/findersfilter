using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui.PartyFinder.Types;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.GeneratedSheets;

namespace FindersFilter;

public interface IFilter
{
    bool AcceptsListing(PartyFinderListing listing);
}

public class AggregateFilter : IFilter
{
    private readonly List<IFilter> filters;

    public AggregateFilter(List<IFilter> filters)
    {
        this.filters = filters;
    }

    public virtual bool AcceptsListing(PartyFinderListing listing) =>
        filters.All(filter => filter.AcceptsListing(listing));
}

public class JoinableMakeupFilter : IFilter
{
    private readonly Configuration configuration;

    public JoinableMakeupFilter(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public virtual bool AcceptsListing(PartyFinderListing listing)
    {
        if (!configuration.PartyMakeupFilter) return true;

        var members = FetchPartyJobs();
        return members == null || ListingSatisfiedBy(listing, members.Select(j => j.JobFlags()).ToList());
    }

    /// <summary>
    /// Check whether or not a PF entry can 'fit' the supplied list of jobs.
    /// </summary>
    private static bool ListingSatisfiedBy(PartyFinderListing listing, List<JobFlags> players)
    {
        // TODO: break down an alliance into its full parties? how does this work?
        if (listing[SearchAreaFlags.AllianceRaid]) return true;

        var filled = listing.RawJobsPresent.ToArray();
        var availableSlots = listing.Slots.Where((_, i) => filled[i] == 0).ToList();

        // If there are more players trying to join than available slots, quit immediately
        if (availableSlots.Count < players.Count) return false;

        return SlotsSatisfiedBy(availableSlots, players);
    }

    private static bool SlotsSatisfiedBy(List<PartyFinderSlot> slots, List<JobFlags> jobs)
    {
        // it is always possible to 'join' with no one. (base case)
        if (jobs.Count == 0) return true;

        // Try to fit the first player into a slot.
        // For each slot that this is possible, recurse with the slot/player removed.
        var playerOne = jobs[0];
        foreach (var slot in slots)
        {
            var intersect = slot.Flag() & playerOne;
            if (intersect == 0) continue; // This player can't fill `slot`

            var newSlots = slots.ToList();
            newSlots.Remove(slot);
            var newJobs = jobs.GetRange(1, jobs.Count - 1);

            if (SlotsSatisfiedBy(newSlots, newJobs)) return true;
        }

        return false;
    }

    public static List<ClassJob>? FetchPartyJobs()
    {
        List<ClassJob> party;

        unsafe
        {
            var infoModule = Framework.Instance()->GetUiModule()->GetInfoModule();
            var partyInfoProxy = (InfoProxyParty*)infoModule->GetInfoProxyById(InfoProxyId.Party);
            if (partyInfoProxy == null)
                return null;

            party = new List<ClassJob>();

            for (uint i = 0; i < partyInfoProxy->InfoProxyCommonList.DataSize; ++i)
            {
                var entry = partyInfoProxy->InfoProxyCommonList.GetEntry(i);
                if (entry == null) continue;

                var job = Dalamud.DataManager.GetExcelSheet<ClassJob>()?.GetRow(entry->Job);
                if (job != null) party.Add(job);
            }
        }

        return party;
    }
}

internal static class PartyFinderSlotExt
{
    // TODO: this should just be exposed in the dalamud API; `Accepting` is a ridiculous property
    internal static JobFlags Flag(this PartyFinderSlot slot)
    {
        return slot.Accepting.Aggregate((JobFlags)0, (aggregate, c) => aggregate | c);
    }
}

internal static class ClassJobExt
{
    internal static JobFlags JobFlags(this ClassJob classJob)
    {
        // This is a bit hacky, but I didn't feel like making an exhaustive switch/case of all jobs.
        var idx = classJob.RowId;
        return idx switch
        {
            // All base classes have index 1-7, and appear in the same order in the bitfield:
            >= 1 and <= 7 => (JobFlags)(1 << (int)idx),
            // then there's a gap of the 11 DoH/L classes,
            // and the jobs start at index 19:
            >= 19 and <= 40 => (JobFlags)(1 << ((int)idx - 11)),
            _ => 0
        };
    }
}
