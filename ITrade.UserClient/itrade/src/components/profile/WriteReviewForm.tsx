import { useState } from 'react';
import { reviewService } from '../../api';

interface WriteReviewModalProps {
    revieweeId: number;
    onClose: () => void;
    onReviewSubmitted: () => void;
}

/** Modal overlay to write a review, styled like CreateProjectModal */
export default function WriteReviewModal({ revieweeId, onClose, onReviewSubmitted }: WriteReviewModalProps) {
    const [title, setTitle] = useState('');
    const [comment, setComment] = useState('');
    const [rating, setRating] = useState(0);
    const [hoveredStar, setHoveredStar] = useState(0);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async () => {
        if (rating === 0) {
            setError('Please select a rating.');
            return;
        }
        if (!title.trim()) {
            setError('Please enter a title.');
            return;
        }

        setIsSubmitting(true);
        setError(null);
        try {
            await reviewService.createReview({
                revieweeId,
                title: title.trim(),
                comment: comment.trim() || undefined,
                rating,
            });
            onReviewSubmitted();
            onClose();
        } catch {
            setError('Failed to submit review. You may have already reviewed this user.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
            <div className="w-full max-w-lg mx-4 bg-[#0f1f3d] border border-white/10 rounded-2xl shadow-2xl overflow-hidden">
                {/* Header */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-white/[0.06]">
                    <h2 className="text-lg font-bold text-slate-200 m-0">Write a Review</h2>
                    <button
                        onClick={onClose}
                        className="w-8 h-8 flex items-center justify-center rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
                    >
                        ✕
                    </button>
                </div>

                {/* Body */}
                <div className="flex flex-col gap-4 px-6 py-5">
                    {error && (
                        <div className="px-4 py-2.5 text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg">
                            {error}
                        </div>
                    )}

                    {/* Star rating picker */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Rating *</label>
                        <div className="flex gap-1">
                            {[1, 2, 3, 4, 5].map(star => (
                                <button
                                    key={star}
                                    type="button"
                                    onMouseEnter={() => setHoveredStar(star)}
                                    onMouseLeave={() => setHoveredStar(0)}
                                    onClick={() => setRating(star)}
                                    className="bg-transparent border-none p-0.5 cursor-pointer transition-transform hover:scale-110"
                                >
                                    <svg
                                        width="24"
                                        height="24"
                                        viewBox="0 0 20 20"
                                        fill={star <= (hoveredStar || rating) ? 'currentColor' : 'none'}
                                        stroke="currentColor"
                                        strokeWidth="1.5"
                                        className={
                                            star <= (hoveredStar || rating)
                                                ? 'text-amber-400'
                                                : 'text-slate-600'
                                        }
                                    >
                                        <path d="M10 1l2.39 4.84 5.34.78-3.87 3.77.91 5.32L10 13.27l-4.77 2.44.91-5.32L2.27 6.62l5.34-.78L10 1z" />
                                    </svg>
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Title */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Title *</label>
                        <input
                            type="text"
                            value={title}
                            onChange={e => setTitle(e.target.value)}
                            placeholder="Summarise your experience…"
                            maxLength={100}
                            className="px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors placeholder:text-slate-600"
                        />
                    </div>

                    {/* Comment (optional) */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-medium text-slate-400">Comment</label>
                        <textarea
                            value={comment}
                            onChange={e => setComment(e.target.value)}
                            placeholder="Share more details…"
                            rows={3}
                            maxLength={500}
                            className="px-4 py-2.5 text-sm text-slate-200 bg-white/[0.04] border border-white/[0.08] rounded-lg outline-none focus:border-blue-500/50 transition-colors resize-none placeholder:text-slate-600"
                        />
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
                        {isSubmitting ? 'Submitting…' : 'Submit Review'}
                    </button>
                </div>
            </div>
        </div>
    );
}
