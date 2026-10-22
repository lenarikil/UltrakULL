using System;
using System.Collections.Generic;
using UltrakULL.json;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// UI-agnostic state of the translation browser: search query, sort mode, the
    /// filtered/sorted visible list and the selected entry. Views subscribe to the
    /// events and render accordingly; DownloadFlow hooks DownloadRequested.
    /// </summary>
    public sealed class BrowserState
    {
        public event Action ListChanged;
        public event Action SelectionChanged;
        public event Action<TranslationEntry> DownloadRequested;
        public event Action RefreshRequested;

        private string _searchQuery = "";
        private CatalogSortMode _sortMode = CatalogSortMode.Popularity;
        private List<TranslationEntry> _visible = new List<TranslationEntry>();
        private string _selectedId;

        public string SearchQuery
        {
            get { return _searchQuery; }
        }

        public CatalogSortMode SortMode
        {
            get { return _sortMode; }
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
                    if (entry != null && string.Equals(entry.id, _selectedId, StringComparison.OrdinalIgnoreCase))
                    {
                        return entry;
                    }
                }
                return null;
            }
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

        public void CycleSortMode()
        {
            switch (_sortMode)
            {
                case CatalogSortMode.Popularity:
                    _sortMode = CatalogSortMode.Language;
                    break;
                case CatalogSortMode.Language:
                    _sortMode = CatalogSortMode.Team;
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
            if (_visible.Count > 0)
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
            _visible = CatalogService.Sort(CatalogService.Search(_searchQuery), _sortMode);

            bool selectionStillValid = false;
            foreach (TranslationEntry entry in _visible)
            {
                if (entry != null && string.Equals(entry.id, _selectedId, StringComparison.OrdinalIgnoreCase))
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