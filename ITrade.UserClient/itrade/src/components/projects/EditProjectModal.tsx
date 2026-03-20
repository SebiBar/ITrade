import { useState, useEffect, useRef } from 'react';
import { projectService, tagService } from '../../api';
import { ProjectStatusType } from '../../types';
import type { ProjectResponse, Tag } from '../../types';

interface EditProjectModalProps {
    project: ProjectResponse;
    onClose: () => void;
    onUpdated: () => void;
}

const STATUS_OPTIONS: { value: number; label: string }[] = [
    { value: ProjectStatusType.Hiring, label: 'Hiring' },
    { value: ProjectStatusType.InProgress, label: 'In Progress' },
    { value: ProjectStatusType.Completed, label: 'Completed' },
    { value: ProjectStatusType.OnHold, label: 'On Hold' },
    { value: ProjectStatusType.Cancelled, label: 'Cancelled' },
];

export default function EditProjectModal({ project, onClose, onUpdated }: EditProjectModalProps) {
    const [name, setName] = useState(project.name);
    const [description, setDescription] = useState(project.description ?? '');
    const [deadline, setDeadline] = useState(project.deadline.slice(0, 10));
    const [status, setStatus] = useState(project.projectStatusTypeId);
    const [tagQuery, setTagQuery] = useState('');
    const [tagResults, setTagResults] = useState<Tag[]>([]);
    const [currentTags, setCurrentTags] = useState<Tag[]>(
        project.tags.map(t => ({ id: t.id, name: t.name }))
    );
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const currentTagsRef = useRef(currentTags);
    currentTagsRef.current = currentTags;
    const panelRef = useRef<HTMLDivElement>(null);

    // Close on click outside
    useEffect(() => {
        const handleClick = (e: MouseEvent) => {
            if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
                onClose();
            }
        };
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [onClose]);

    // Search tags with debounce
    useEffect(() => {
        if (tagQuery.trim().length < 2) {
            setTagResults([]);
            return;
        }
        let cancelled = false;
        const timeout = setTimeout(() => {
            tagService
                .searchTags(tagQuery.trim())
                .then(results => {
                    if (!cancelled) {
                        setTagResults(results.filter(t => !currentTagsRef.current.some(s => s.id === t.id)));
                    }
                })
                .catch(() => { /* ignore */ });
        }, 300);
        return () => {
            cancelled = true;
            clearTimeout(timeout);
        };
    }, [tagQuery]);

    const addTag = async (tag: Tag) => {
        setCurrentTags(prev => [...prev, tag]);
        setTagQuery('');
        setTagResults([]);
        try {
            await projectService.addProjectTag(project.id, tag.id);
        } catch {
            // revert on failure
            setCurrentTags(prev => prev.filter(t => t.id !== tag.id));
        }
    };

    const removeTag = async (tagId: number) => {
        const removed = currentTags.find(t => t.id === tagId);
        setCurrentTags(prev => prev.filter(t => t.id !== tagId));
        try {
            await projectService.removeProjectTag(project.id, tagId);
        } catch {
            // revert on failure
            if (removed) setCurrentTags(prev => [...prev, removed]);
        }
    };

    const handleSubmit = async () => {
        if (!name.trim()) {
            setError('Project name is required.');
            return;
        }
        setIsSubmitting(true);
        setError(null);
        try {
            await projectService.updateProject(project.id, {
                name: name.trim(),
                description: description.trim() || undefined,
                deadline,
                status: status as typeof ProjectStatusType[keyof typeof ProjectStatusType],
            });
            onUpdated();
        } catch {
            setError('Failed to update project. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
            <div ref={panelRef} className="w-full max-w-lg mx-4 bg-[#0f1f3d] border border-white/10 rounded-2xl shadow-2xl overflow-hidden">
                {/* Header */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-white/[0.06]">
                    <h2 className="text-lg font-bold text-slate-200 m-0">Edit Project</h2>
                    <button
                        onClick={onClose}
                        className="w-8 h-8 flex items-center justify-center rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
                    >
                        ✕
                    </button>
                </div>

                {/* Body */}
                <div className="flex flex-col gap-4 px-6 py-5 max-h-[60vh] overflow-y-auto">
                    {error && (
                        <div className="px-4 py-2.5 text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg">
                            {error}
                        </div>
                    )}

                    {/* Name */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Project Name *</label>
                        <input
                            type="text"
                            value={name}
                            onChange={e => setName(e.target.value)}
                            className="px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors placeholder:text-slate-600"
                        />
                    </div>

                    {/* Description */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Description</label>
                        <textarea
                            value={description}
                            onChange={e => setDescription(e.target.value)}
                            rows={3}
                            className="px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors resize-none placeholder:text-slate-600"
                        />
                    </div>

                    {/* Deadline */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Deadline</label>
                        <input
                            type="date"
                            value={deadline}
                            onChange={e => setDeadline(e.target.value)}
                            className="px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors [color-scheme:dark]"
                        />
                    </div>

                    {/* Status */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Status</label>
                        <select
                            value={status}
                            onChange={e => setStatus(Number(e.target.value))}
                            className="px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors [color-scheme:dark] cursor-pointer"
                        >
                            {STATUS_OPTIONS.map(opt => (
                                <option key={opt.value} value={opt.value} className="bg-[#0f1f3d]">
                                    {opt.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Tags */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Tags</label>
                        {currentTags.length > 0 && (
                            <div className="flex flex-wrap gap-1.5 mb-1">
                                {currentTags.map(tag => (
                                    <span
                                        key={tag.id}
                                        className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium rounded-lg bg-indigo-500/15 text-indigo-300 border border-indigo-500/20"
                                    >
                                        {tag.name}
                                        <button
                                            onClick={() => removeTag(tag.id)}
                                            className="ml-0.5 text-indigo-400 hover:text-white cursor-pointer bg-transparent border-none p-0 text-xs"
                                        >
                                            ×
                                        </button>
                                    </span>
                                ))}
                            </div>
                        )}
                        <div className="relative">
                            <input
                                type="text"
                                value={tagQuery}
                                onChange={e => setTagQuery(e.target.value)}
                                placeholder="Search tags…"
                                className="w-full px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors placeholder:text-slate-600"
                            />
                            {tagResults.length > 0 && (
                                <div className="absolute top-full left-0 right-0 mt-1 max-h-36 overflow-y-auto bg-[#0f1f3d] border border-white/10 rounded-lg shadow-xl z-10">
                                    {tagResults.map(tag => (
                                        <button
                                            key={tag.id}
                                            onClick={() => addTag(tag)}
                                            className="w-full text-left px-4 py-2 text-sm text-slate-300 hover:bg-white/5 hover:text-white transition-colors cursor-pointer bg-transparent border-none"
                                        >
                                            {tag.name}
                                        </button>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Footer */}
                <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-white/[0.06]">
                    <button
                        onClick={onClose}
                        disabled={isSubmitting}
                        className="px-4 py-2 text-sm font-medium text-slate-400 hover:text-white transition-colors cursor-pointer bg-transparent border-none"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={handleSubmit}
                        disabled={isSubmitting}
                        className="px-5 py-2 text-sm font-semibold text-white bg-gradient-to-r from-blue-500 to-indigo-600 rounded-lg hover:brightness-110 active:scale-[0.98] transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed border-none shadow-[0_2px_12px_rgba(79,125,255,0.3)]"
                    >
                        {isSubmitting ? 'Saving…' : 'Save Changes'}
                    </button>
                </div>
            </div>
        </div>
    );
}
