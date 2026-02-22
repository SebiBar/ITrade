import { useState } from 'react';
import { userService, tagService } from '../../api';
import type { UserProfileTagResponse, UserProfileLinkResponse, Tag } from '../../types';

interface ProfileDetailsProps {
    tags: UserProfileTagResponse[];
    links: UserProfileLinkResponse[];
    isOwnProfile?: boolean;
    userRole?: string;
    onTagAdded?: () => void;
    onTagRemoved?: () => void;
    onLinkAdded?: () => void;
    onLinkRemoved?: () => void;
}

/** Displays the user's skill tags and external links, with edit controls when viewing own profile */
export default function ProfileDetails({
    tags,
    links,
    isOwnProfile,
    userRole,
    onTagAdded,
    onTagRemoved,
    onLinkAdded,
    onLinkRemoved,
}: ProfileDetailsProps) {
    // ── Tag adding state ──────────────────────────────────────────────────────
    const [tagQuery, setTagQuery] = useState('');
    const [tagResults, setTagResults] = useState<Tag[]>([]);
    const [isSearchingTags, setIsSearchingTags] = useState(false);
    const [tagError, setTagError] = useState<string | null>(null);

    // ── Link adding state ─────────────────────────────────────────────────────
    const [newLinkUrl, setNewLinkUrl] = useState('');
    const [isAddingLink, setIsAddingLink] = useState(false);
    const [linkError, setLinkError] = useState<string | null>(null);

    // ── Tag handlers ──────────────────────────────────────────────────────────
    const handleTagSearch = async (query: string) => {
        setTagQuery(query);
        setTagError(null);
        if (query.trim().length < 2) {
            setTagResults([]);
            return;
        }
        setIsSearchingTags(true);
        try {
            const results = await tagService.searchTags(query.trim());
            // Filter out tags the user already has
            const existingIds = new Set(tags.map(t => t.id));
            setTagResults(results.filter(t => !existingIds.has(t.id)));
        } catch {
            setTagError('Failed to search tags.');
        } finally {
            setIsSearchingTags(false);
        }
    };

    const handleAddTag = async (tagId: number) => {
        setTagError(null);
        try {
            await userService.addProfileTag(tagId);
            setTagQuery('');
            setTagResults([]);
            onTagAdded?.();
        } catch {
            setTagError('Failed to add tag.');
        }
    };

    const handleRemoveTag = async (tagId: number) => {
        setTagError(null);
        try {
            await userService.removeProfileTag(tagId);
            onTagRemoved?.();
        } catch {
            setTagError('Failed to remove tag.');
        }
    };

    // ── Link handlers ─────────────────────────────────────────────────────────
    const handleAddLink = async () => {
        const url = newLinkUrl.trim();
        if (!url) return;
        setLinkError(null);
        setIsAddingLink(true);
        try {
            await userService.createProfileLink(url);
            setNewLinkUrl('');
            onLinkAdded?.();
        } catch {
            setLinkError('Failed to add link.');
        } finally {
            setIsAddingLink(false);
        }
    };

    const handleRemoveLink = async (linkId: number) => {
        setLinkError(null);
        try {
            await userService.removeProfileLink(linkId);
            onLinkRemoved?.();
        } catch {
            setLinkError('Failed to remove link.');
        }
    };

    // ── Render ────────────────────────────────────────────────────────────────

    const isClient = userRole === 'Client';
    const showTags = !isClient && (tags.length > 0 || isOwnProfile);
    const showLinks = links.length > 0 || isOwnProfile;

    if (!showTags && !showLinks) return null;

    return (
        <>
            {/* Tags */}
            {showTags && (
                <div>
                    <h3 className="text-[0.65rem] font-semibold text-slate-500 uppercase tracking-wider m-0 mb-2">
                        Skills
                    </h3>
                    <div className="flex flex-wrap gap-2">
                        {tags.map(tag => (
                            <span
                                key={tag.id}
                                className="inline-flex items-center gap-1.5 px-2.5 py-1 text-xs font-medium rounded-lg bg-indigo-500/10 text-indigo-300 border border-indigo-500/15"
                            >
                                {tag.name}
                                {isOwnProfile && (
                                    <button
                                        onClick={() => handleRemoveTag(tag.id)}
                                        className="text-indigo-400/50 hover:text-red-400 bg-transparent border-none p-0 cursor-pointer transition-colors leading-none text-sm"
                                        title="Remove tag"
                                    >
                                        &times;
                                    </button>
                                )}
                            </span>
                        ))}
                    </div>

                    {/* Add tag input */}
                    {isOwnProfile && (
                        <div className="mt-2.5 relative">
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
                                            onClick={() => handleAddTag(tag.id)}
                                            className="w-full text-left px-3 py-2 text-xs text-slate-300 hover:bg-white/[0.06] hover:text-white bg-transparent border-none cursor-pointer transition-colors"
                                        >
                                            {tag.name}
                                        </button>
                                    ))}
                                </div>
                            )}
                            {tagError && (
                                <p className="text-[0.65rem] text-red-400 m-0 mt-1">{tagError}</p>
                            )}
                        </div>
                    )}
                </div>
            )}

            {/* Links */}
            {showLinks && (
                <div>
                    <h3 className="text-[0.65rem] font-semibold text-slate-500 uppercase tracking-wider m-0 mb-2">
                        Links
                    </h3>
                    <div className="flex flex-wrap gap-3">
                        {links.map(link => (
                            <span key={link.id} className="inline-flex items-center gap-1.5">
                                <a
                                    href={link.url}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="inline-flex items-center gap-1.5 text-xs font-medium text-blue-400 hover:text-blue-300 transition-colors"
                                >
                                    <svg width="12" height="12" viewBox="0 0 16 16" fill="none" className="shrink-0">
                                        <path
                                            d="M6.5 3.5H3.5a1 1 0 00-1 1v8a1 1 0 001 1h8a1 1 0 001-1v-3m-4-7h5m0 0v5m0-5L7 9"
                                            stroke="currentColor" strokeWidth="1.25" strokeLinecap="round" strokeLinejoin="round"
                                        />
                                    </svg>
                                    {(() => {
                                        try {
                                            return new URL(link.url).hostname.replace('www.', '');
                                        } catch {
                                            return link.url;
                                        }
                                    })()}
                                </a>
                                {isOwnProfile && (
                                    <button
                                        onClick={() => handleRemoveLink(link.id)}
                                        className="text-slate-600 hover:text-red-400 bg-transparent border-none p-0 cursor-pointer transition-colors leading-none text-sm"
                                        title="Remove link"
                                    >
                                        &times;
                                    </button>
                                )}
                            </span>
                        ))}
                    </div>

                    {/* Add link input */}
                    {isOwnProfile && (
                        <div className="mt-2.5 flex gap-2 items-center">
                            <input
                                type="url"
                                value={newLinkUrl}
                                onChange={e => setNewLinkUrl(e.target.value)}
                                onKeyDown={e => e.key === 'Enter' && handleAddLink()}
                                placeholder="https://…"
                                className="flex-1 max-w-xs px-3 py-1.5 text-xs text-slate-300 placeholder-slate-600 bg-white/[0.03] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/30 transition-colors"
                            />
                            <button
                                onClick={handleAddLink}
                                disabled={isAddingLink || !newLinkUrl.trim()}
                                className="px-3 py-1.5 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
                            >
                                {isAddingLink ? 'Adding…' : 'Add'}
                            </button>
                            {linkError && (
                                <p className="text-[0.65rem] text-red-400 m-0">{linkError}</p>
                            )}
                        </div>
                    )}
                </div>
            )}
        </>
    );
}
