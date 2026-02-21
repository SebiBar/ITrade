interface StarRatingProps {
    rating: number;
}

/** Renders 1–5 filled/unfilled stars */
export default function StarRating({ rating }: StarRatingProps) {
    return (
        <span className="inline-flex gap-0.5">
            {[1, 2, 3, 4, 5].map(star => (
                <svg
                    key={star}
                    width="14"
                    height="14"
                    viewBox="0 0 20 20"
                    fill={star <= rating ? 'currentColor' : 'none'}
                    stroke="currentColor"
                    strokeWidth="1.5"
                    className={star <= rating ? 'text-amber-400' : 'text-slate-600'}
                >
                    <path d="M10 1l2.39 4.84 5.34.78-3.87 3.77.91 5.32L10 13.27l-4.77 2.44.91-5.32L2.27 6.62l5.34-.78L10 1z" />
                </svg>
            ))}
        </span>
    );
}
