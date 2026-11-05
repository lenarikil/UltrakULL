using System;
using System.Collections.Generic;
using UltrakULL.json;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// UI-agnostic state of the translation browser: search query, sort mode,
    /// language filter, the filtered/sorted visible list and the selected entry.
    /// Views subscribe to the events and render accordingly; DownloadFlow hooks
    /// DownloadRequested.
    /// </summary>
    public sealed class BrowserState
    {
        public event Action ListChanged;
        public event Action SelectionChanged;
        public event Action<TranslationEntry> DownloadRequested;
        public event Action RefreshRequested;
        public event Action InstallingChanged;

        private string _searchQuery = "";
        private CatalogSortMode _sortMode = CatalogSortMode.Popularity;

        /// <summary>Language tag filter (null = all languages).</summary>
        private string _filterLanguage;

        private List<TranslationEntry> _visible = new List<TranslationEntry>();
        private string _selectedId;

        // Product tour override: when set, VisibleEntries returns these dummy
        // entries instead of recomputing from the real catalog. Cleared at the
        // end of the tour.
        private List<TranslationEntry> _visibleOverride;

        // Progress of entries currently being installed (multiple concurrent installs).
        private readonly Dictionary<string, InstallProgress> _installingEntries = new Dictionary<string, InstallProgress>();

        public string SearchQuery
        {
            get { return _searchQuery; }
        }

        public CatalogSortMode SortMode
        {
            get { return _sortMode; }
        }

        public string FilterLanguage
        {
            get { return _filterLanguage; }
        }

        public IReadOnlyList<TranslationEntry> VisibleEntries
        {
            get { return _visible; }
        }

        public string SelectedId
        {
            get { return _selectedId; }
        }

        public TranslationEntry SelectedEntry
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedId))
                {
                    return null;
                }
                foreach (TranslationEntry entry in _visible)
                {
                    if (entry != null && string.Equals(entry.id, _selectedId, StringComparison.Ordinal))
                    {
                        return entry;
                    }
                }
                return null;
            }
        }

        /// <summary>Ids of the entries currently being installed, or empty when idle.</summary>
        public IReadOnlyDictionary<string, InstallProgress> InstallingEntries
        {
            get { return _installingEntries; }
        }

        /// <summary>True when at least one entry is being installed.</summary>
        public bool IsInstalling
        {
            get { return _installingEntries.Count > 0; }
        }

        /// <summary>Progress of the entry with <paramref name="entryId"/> being installed, or null.</summary>
        public InstallProgress GetInstallingProgress(string entryId)
        {
            InstallProgress progress;
            _installingEntries.TryGetValue(entryId, out progress);
            return progress;
        }

        public void SetSearch(string query)
        {
            _searchQuery = query ?? "";
            Recompute();
        }

        public void SetSortMode(CatalogSortMode mode)
        {
            _sortMode = mode;
            Recompute();
        }

        public void SetFilterLanguage(string languageTag)
        {
            _filterLanguage = string.IsNullOrEmpty(languageTag) ? null : languageTag;
            Recompute();
        }

        public void CycleSortMode()
        {
            switch (_sortMode)
            {
                case CatalogSortMode.Popularity:
                    _sortMode = CatalogSortMode.Updated;
                    break;
                case CatalogSortMode.Updated:
                    _sortMode = CatalogSortMode.Language;
                    break;
                default:
                    _sortMode = CatalogSortMode.Popularity;
                    break;
            }
            Recompute();
        }

        public void SetSelected(string id)
        {
            if (string.Equals(_selectedId, id, StringComparison.Ordinal))
            {
                return;
            }
            _selectedId = id;
            NotifySelectionChanged();
        }

        /// <summary>Selects the first visible entry, or clears the selection when empty.</summary>
        public void SelectFirst()
        {
            if (_visible.Count > 0 && _visible[0] != null)
            {
                SetSelected(_visible[0].id);
            }
            else
            {
                SetSelected(null);
            }
        }

        public void Recompute()
        {
            // Product tour override: keep the dummy entries without touching
            // the real catalog.
            if (_visibleOverride != null)
            {
                _visible = new List<TranslationEntry>(_visibleOverride);
                NotifyListChanged();
                return;
            }

            List<TranslationEntry> searched = CatalogService.Search(_searchQuery);

            if (_filterLanguage != null)
            {
                List<TranslationEntry> filtered = new List<TranslationEntry>(searched.Count);
                foreach (TranslationEntry entry in searched)
                {
                    if (entry != null && string.Equals(entry.languageTag, _filterLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        filtered.Add(entry);
                    }
                }
                searched = filtered;
            }

            _visible = CatalogService.Sort(searched, _sortMode);

            bool selectionStillValid = false;
            foreach (TranslationEntry entry in _visible)
            {
                if (entry != null && string.Equals(entry.id, _selectedId, StringComparison.Ordinal))
                {
                    selectionStillValid = true;
                    break;
                }
            }
            if (!selectionStillValid)
            {
                _selectedId = null;
                NotifySelectionChanged();
            }

            NotifyListChanged();
        }

        public void RequestDownload(TranslationEntry entry)
        {
            if (DownloadRequested != null)
            {
                DownloadRequested(entry);
            }
        }

        public void RequestRefresh()
        {
            if (RefreshRequested != null)
            {
                RefreshRequested();
            }
        }

        public void SetInstalling(string entryId, InstallProgress progress)
        {
            bool changed = !_installingEntries.ContainsKey(entryId) ||
                !Equals(_installingEntries[entryId], progress);
            _installingEntries[entryId] = progress;
            if (changed)
            {
                NotifyInstallingChanged();
            }
        }

        public void ClearInstalling(string entryId)
        {
            if (_installingEntries.Remove(entryId))
            {
                NotifyInstallingChanged();
            }
        }

        public void ClearAllInstalling()
        {
            if (_installingEntries.Count > 0)
            {
                _installingEntries.Clear();
                NotifyInstallingChanged();
            }
        }

        /// <summary>Temporarily replaces the visible entries with dummy data
        /// for the product tour. Call <see cref="ClearVisibleEntriesOverride"/>
        /// to restore normal behavior.</summary>
        public void SetVisibleEntriesOverride(List<TranslationEntry> entries)
        {
            _visibleOverride = entries ?? new List<TranslationEntry>();
            _visible = new List<TranslationEntry>(_visibleOverride);
            NotifyListChanged();
        }

        /// <summary>Restores normal visible-entry computation after a tour
        /// override.</summary>
        public void ClearVisibleEntriesOverride()
        {
            _visibleOverride = null;
        }

        private void NotifyInstallingChanged()
        {
            if (InstallingChanged != null)
            {
                InstallingChanged();
            }
        }

        private void NotifyListChanged()
        {
            if (ListChanged != null)
            {
                ListChanged();
            }
        }

        private void NotifySelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged();
            }
        }
    }
}
