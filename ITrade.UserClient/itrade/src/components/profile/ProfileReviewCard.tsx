import { useNavigate } from 'react-router-dom';
import type { ReviewResponse } from '../../types';
import StarRating from './StarRating';

interface ProfileReviewCardProps {
    review: ReviewResponse;
}

const formatDate = (iso: string) =>
    new Date(iso).toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
    });

/** Review card showing reviewer, star rating, and comment */
export default function ProfileReviewCard({ review }: ProfileReviewCardProps) {
    const navigate = useNavigate();

    return (
        <div className="flex flex-col gap-2 p-4 bg-white/[0.03] border border-white/[0.06] rounded-xl">
            <div className="flex items-center justify-between gap-3">
                <button
                    onClick={() => navigate(`/users/${review.reviewerId}`)}
                    className="flex items-center gap-2 min-w-0 bg-transparent border-none p-0 cursor-pointer group"
                >
                    <div className="w-7 h-7 rounded-full bg-gradient-to-br from-blue-500/30 to-indigo-500/30 border border-blue-500/20 flex items-center justify-center shrink-0 group-hover:border-blue-500/40 transition-colors">
                        <span className="text-[0.6rem] font-bold text-blue-300">
                            {review.reviewerUsername.charAt(0).toUpperCase()}
                        </span>
                    </div>
                    <span className="text-xs font-medium text-slate-300 truncate group-hover:text-blue-400 transition-colors">
                        {review.reviewerUsername}
                    </span>
                </button>
                <StarRating rating={review.rating} />
            </div>
            {review.title && (
                <h4 className="text-sm font-semibold text-slate-200 m-0">{review.title}</h4>
            )}
            {review.comment && (
                <p className="text-xs text-slate-400 m-0 leading-relaxed">{review.comment}</p>
            )}
            <span className="text-[0.6rem] text-slate-600">{formatDate(review.createdAt)}</span>
        </div>
    );
}
