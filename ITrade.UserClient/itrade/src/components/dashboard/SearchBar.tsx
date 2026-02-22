import { useState, useRef, useEffect, type FormEvent } from 'react';
import {
    SearchEntityType,
    SearchSortBy,
    SortDirection,
    ProjectStatusType,
} from '../../types';
import type { SearchRequest, SearchResponse, Tag } from '../../types';
import { discoveryService, tagService } from '../../api';

interface SearchBarProps {
    /** Called when search results arrive */
    onResults: (results: SearchResponse | null) => void;
    /** Called when search loading state changes */
    onLoading?: (loading: boolean) => void;
}

const entityLabels: Record<number, string> = {
    [SearchEntityType.All]: 'All',
    [SearchEntityType.Projects]: 'Projects',
    [SearchEntityType.Users]: 'Users',
};

const sortLabels: Record<number, string> = {
    [SearchSortBy.Relevance]: 'Relevance',
    [SearchSortBy.CreatedAt]: 'Date Created',
    [SearchSortBy.Name]: 'Name',
    [SearchSortBy.Deadline]: 'Deadline',
};

const statusLabels: Record<number, string> = {
    [ProjectStatusType.Hiring]: 'Hiring',
    [ProjectStatusType.InProgress]: 'In Progress',
    [ProjectStatusType.Completed]: 'Completed',
    [ProjectStatusType.OnHold]: 'On Hold',
    [ProjectStatusType.Cancelled]: 'Cancelled',
};

/** Dashboard search bar with expandable filter/sort panel */
export default function SearchBar({ onResults, onLoading }: SearchBarProps) {
    const [query, setQuery] = useState('');
    const [showFilters, setShowFilters] = useState(false);
    const [isSearching, setIsSearching] = useState(false);

    // Filters
    const [entityType, setEntityType] = useState<number>(SearchEntityType.All);
    const [sortBy, setSortBy] = useState<number>(SearchSortBy.Relevance);
    const [sortDirection, setSortDirection] = useState<number>(SortDirection.Descending);
    const [projectStatus, setProjectStatus] = useState<number | undefined>(undefined);
    const [deadlineFrom, setDeadlineFrom] = useState('');
    const [deadlineTo, setDeadlineTo] = useState('');

    // Tag filter state
    const [selectedTags, setSelectedTags] = useState<Tag[]>([]);
    const [tagQuery, setTagQuery] = useState('');
    const [tagResults, setTagResults] = useState<Tag[]>([]);
    const [isSearchingTags, setIsSearchingTags] = useState(false);

    const filtersRef = useRef<HTMLDivElement>(null);

    // Close filters on outside click
    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (filtersRef.current && !filtersRef.current.contains(e.target as Node)) {
                setShowFilters(false);
            }
        };
        document.addEventListener('mousedown', handler);
        return () => document.removeEventListener('mousedown', handler);
    }, []);

    const handleTagSearch = async (query: string) => {
        setTagQuery(query);
        if (query.trim().length < 2) {
            setTagResults([]);
            return;
        }
        setIsSearchingTags(true);
        try {
            const results = await tagService.searchTags(query.trim());
            const existingIds = new Set(selectedTags.map(t => t.id));
            setTagResults(results.filter(t => !existingIds.has(t.id)));
        } catch {
            // fail silently
        } finally {
            setIsSearchingTags(false);
        }
    };

    const handleAddTag = (tag: Tag) => {
        setSelectedTags(prev => [...prev, tag]);
        setTagQuery('');
        setTagResults([]);
    };

    const handleRemoveTag = (tagId: number) => {
        setSelectedTags(prev => prev.filter(t => t.id !== tagId));
    };

    const handleSearch = async (e?: FormEvent) => {
        e?.preventDefault();
        if (!query.trim() && !projectStatus && !deadlineFrom && !deadlineTo && selectedTags.length === 0) {
            onResults(null);
            return;
        }

        setIsSearching(true);
        onLoading?.(true);

        const params: SearchRequest = {
            query: query.trim() || undefined,
            entityType: entityType as SearchRequest['entityType'],
            sortBy: sortBy as SearchRequest['sortBy'],
            sortDirection: sortDirection as SearchRequest['sortDirection'],
            projectStatusId: projectStatus,
            tagIds: selectedTags.length > 0 ? selectedTags.map(t => t.id) : undefined,
            deadlineFrom: deadlineFrom || undefined,
            deadlineTo: deadlineTo || undefined,
            page: 1,
            pageSize: 20,
        };

        try {
            const results = await discoveryService.search(params);
            onResults(results);
        } catch {
            // fail silently
        } finally {
            setIsSearching(false);
            onLoading?.(false);
        }
    };

    const handleClear = () => {
        setQuery('');
        setEntityType(SearchEntityType.All);
        setSortBy(SearchSortBy.Relevance);
        setSortDirection(SortDirection.Descending);
        setProjectStatus(undefined);
        setDeadlineFrom('');
        setDeadlineTo('');
        setSelectedTags([]);
        setTagQuery('');
        setTagResults([]);
        onResults(null);
    };

    const activeFilterCount = [
        entityType !== SearchEntityType.All,
        sortBy !== SearchSortBy.Relevance,
        sortDirection !== SortDirection.Descending,
        projectStatus !== undefined,
        selectedTags.length > 0,
        !!deadlineFrom,
        !!deadlineTo,
    ].filter(Boolean).length;

    return (
        <div ref={filtersRef} className="relative w-full">
            {/* Search input row */}
            <form onSubmit={handleSearch} className="flex items-center gap-2">
                <div className="relative flex-1">
                    {/* Search icon */}
                    <svg
                        className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500 pointer-events-none"
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth={2}
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    >
                        <circle cx="11" cy="11" r="8" />
                        <path d="M21 21l-4.35-4.35" />
                    </svg>
                    <input
                        type="text"
                        value={query}
                        onChange={e => setQuery(e.target.value)}
                        placeholder="Search projects, specialists…"
                        className="w-full pl-10 pr-4 py-2.5 bg-white/[0.05] border border-white/10 rounded-xl text-sm text-slate-200 placeholder:text-slate-600 outline-none focus:border-blue-500/50 focus:bg-blue-500/[0.04] focus:ring-2 focus:ring-blue-500/15 transition-all"
                    />
                </div>

                {/* Filter toggle */}
                <button
                    type="button"
                    onClick={() => setShowFilters(prev => !prev)}
                    className={`relative p-2.5 rounded-xl border transition-colors cursor-pointer ${showFilters || activeFilterCount > 0
                        ? 'bg-blue-500/10 border-blue-500/25 text-blue-400'
                        : 'bg-white/[0.05] border-white/10 text-slate-400 hover:text-slate-300 hover:bg-white/[0.08]'
                        }`}
                    title="Filters & Sort"
                >
                    <svg
                        className="w-4 h-4"
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth={2}
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    >
                        <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
                    </svg>
                    {activeFilterCount > 0 && (
                        <span className="absolute -top-1 -right-1 min-w-[1rem] h-4 px-0.5 bg-blue-500 text-white text-[0.55rem] rounded-full flex items-center justify-center font-bold">
                            {activeFilterCount}
                        </span>
                    )}
                </button>

                {/* Search button */}
                <button
                    type="submit"
                    disabled={isSearching}
                    className="px-5 py-2.5 bg-gradient-to-br from-[#3b5bdb] to-[#4f7dff] text-white text-sm font-semibold rounded-xl border-0 shadow-[0_4px_20px_rgba(79,125,255,0.25)] hover:opacity-90 transition-opacity cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    {isSearching ? 'Searching…' : 'Search'}
                </button>
            </form>

            {/* Filters panel */}
            {showFilters && (
                <div className="absolute top-full left-0 right-0 mt-2 p-5 bg-[#0f1f3d] border border-white/10 rounded-xl shadow-2xl z-40">
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                        {/* Entity type */}
                        <FilterSelect
                            label="Search in"
                            value={entityType}
                            onChange={v => setEntityType(Number(v))}
                            options={entityLabels}
                        />

                        {/* Sort by */}
                        <FilterSelect
                            label="Sort by"
                            value={sortBy}
                            onChange={v => setSortBy(Number(v))}
                            options={sortLabels}
                        />

                        {/* Sort direction */}
                        <FilterSelect
                            label="Direction"
                            value={sortDirection}
                            onChange={v => setSortDirection(Number(v))}
                            options={{
                                [SortDirection.Ascending]: 'Ascending',
                                [SortDirection.Descending]: 'Descending',
                            }}
                        />

                        {/* Project status */}
                        <FilterSelect
                            label="Project status"
                            value={projectStatus ?? ''}
                            onChange={v => setProjectStatus(v === '' ? undefined : Number(v))}
                            options={statusLabels}
                            placeholder="Any"
                        />

                        {/* Deadline from */}
                        <div className="flex flex-col gap-1.5">
                            <label className="text-[0.65rem] font-semibold text-slate-400 uppercase tracking-wider">
                                Deadline from
                            </label>
                            <input
                                type="date"
                                value={deadlineFrom}
                                onChange={e => setDeadlineFrom(e.target.value)}
                                className="px-3 py-2 bg-white/[0.05] border border-white/10 rounded-lg text-xs text-slate-300 outline-none focus:border-blue-500/50 transition-colors [color-scheme:dark]"
                            />
                        </div>

                        {/* Deadline to */}
                        <div className="flex flex-col gap-1.5">
                            <label className="text-[0.65rem] font-semibold text-slate-400 uppercase tracking-wider">
                                Deadline to
                            </label>
                            <input
                                type="date"
                                value={deadlineTo}
                                onChange={e => setDeadlineTo(e.target.value)}
                                className="px-3 py-2 bg-white/[0.05] border border-white/10 rounded-lg text-xs text-slate-300 outline-none focus:border-blue-500/50 transition-colors [color-scheme:dark]"
                            />
                        </div>
                    </div>

                    {/* Tags filter */}
                    <div className="mt-4 pt-3 border-t border-white/[0.06]">
                        <label className="text-[0.65rem] font-semibold text-slate-400 uppercase tracking-wider">
                            Filter by tags
                        </label>
                        {selectedTags.length > 0 && (
                            <div className="flex flex-wrap gap-1.5 mt-2">
                                {selectedTags.map(tag => (
                                    <span
                                        key={tag.id}
                                        className="inline-flex items-center gap-1 px-2 py-0.5 text-xs font-medium rounded-md bg-indigo-500/15 text-indigo-300 border border-indigo-500/20"
                                    >
                                        {tag.name}
                                        <button
                                            type="button"
                                            onClick={() => handleRemoveTag(tag.id)}
                                            className="text-indigo-400/60 hover:text-red-400 bg-transparent border-none p-0 cursor-pointer transition-colors leading-none text-sm"
                                        >
                                            &times;
                                        </button>
                                    </span>
                                ))}
                            </div>
                        )}
                        <div className="relative mt-2">
                            <input
                                type="text"
                                value={tagQuery}
                                onChange={e => handleTagSearch(e.target.value)}
                                placeholder="Search tags to add…"
                                className="w-full max-w-xs px-3 py-1.5 text-xs text-slate-300 placeholder-slate-600 bg-white/[0.03] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/30 transition-colors"
                            />
                            {isSearchingTags && (
                                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-[0.6rem] text-slate-600">Searching…</span>
                            )}
                            {tagResults.length > 0 && (
                                <div className="absolute z-10 mt-1 w-full max-w-xs bg-[#0f1f3d] border border-white/10 rounded-lg shadow-xl overflow-hidden">
                                    {tagResults.map(tag => (
                                        <button
                                            key={tag.id}
                                            type="button"
                                            onClick={() => handleAddTag(tag)}
                                            className="w-full text-left px-3 py-2 text-xs text-slate-300 hover:bg-white/[0.06] hover:text-white bg-transparent border-none cursor-pointer transition-colors"
                                        >
                                            {tag.name}
                                        </button>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Filter actions */}
                    <div className="flex items-center justify-end gap-3 mt-4 pt-3 border-t border-white/[0.06]">
                        <button
                            type="button"
                            onClick={handleClear}
                            className="px-4 py-1.5 text-xs font-semibold text-slate-400 bg-white/[0.05] border border-white/10 rounded-lg hover:bg-white/[0.08] transition-colors cursor-pointer"
                        >
                            Clear all
                        </button>
                        <button
                            type="button"
                            onClick={() => {
                                setShowFilters(false);
                                handleSearch();
                            }}
                            className="px-4 py-1.5 text-xs font-semibold text-white bg-gradient-to-br from-[#3b5bdb] to-[#4f7dff] rounded-lg border-0 hover:opacity-90 transition-opacity cursor-pointer"
                        >
                            Apply filters
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

// ─── Helper select component ──────────────────────────────────────────────────

function FilterSelect({
    label,
    value,
    onChange,
    options,
    placeholder,
}: {
    label: string;
    value: string | number;
    onChange: (value: string) => void;
    options: Record<number | string, string>;
    placeholder?: string;
}) {
    return (
        <div className="flex flex-col gap-1.5">
            <label className="text-[0.65rem] font-semibold text-slate-400 uppercase tracking-wider">
                {label}
            </label>
            <select
                value={value}
                onChange={e => onChange(e.target.value)}
                className="px-3 py-2 bg-[#0f1f3d] border border-white/10 rounded-lg text-xs text-slate-300 outline-none focus:border-blue-500/50 transition-colors cursor-pointer appearance-none [color-scheme:dark]"
            >
                {placeholder && <option value="" className="bg-[#0f1f3d] text-slate-300">{placeholder}</option>}
                {Object.entries(options).map(([k, v]) => (
                    <option key={k} value={k} className="bg-[#0f1f3d] text-slate-300">
                        {v}
                    </option>
                ))}
            </select>
        </div>
    );
}
