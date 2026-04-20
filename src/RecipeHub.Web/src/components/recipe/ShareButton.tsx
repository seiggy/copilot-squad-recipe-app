import { useEffect, useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Button } from '../ui';
import { apiClient } from '../../api';
import type { ShareDto } from '../../api';
import styles from './ShareButton.module.css';

export interface ShareButtonProps {
  recipeId: number;
}

function buildShareUrl(dto: ShareDto): string {
  if (dto.url && /^https?:\/\//.test(dto.url)) {
    return dto.url;
  }
  if (typeof window !== 'undefined') {
    const path = dto.url && dto.url.length > 0 ? dto.url : `/shared/${dto.token}`;
    return `${window.location.origin}${path.startsWith('/') ? path : `/${path}`}`;
  }
  return dto.url || `/shared/${dto.token}`;
}

export function ShareButton({ recipeId }: ShareButtonProps) {
  const [copied, setCopied] = useState(false);

  const mutation = useMutation<ShareDto, Error, void>({
    mutationFn: () => apiClient.shareRecipe(recipeId),
    onSuccess: async (dto) => {
      const url = buildShareUrl(dto);
      try {
        await navigator.clipboard.writeText(url);
        setCopied(true);
      } catch {
        setCopied(false);
      }
    },
  });

  useEffect(() => {
    if (!copied) return;
    const handle = window.setTimeout(() => setCopied(false), 2000);
    return () => window.clearTimeout(handle);
  }, [copied]);

  return (
    <span className={styles.wrapper}>
      <Button
        variant="ghost"
        onClick={() => mutation.mutate()}
        loading={mutation.isPending}
      >
        Share
      </Button>
      {copied ? (
        <span className={styles.feedback} role="status">
          Copied!
        </span>
      ) : null}
      {mutation.isError ? (
        <span className={styles.error} role="alert">
          Couldn't create share link. Please try again.
        </span>
      ) : null}
    </span>
  );
}

export default ShareButton;
