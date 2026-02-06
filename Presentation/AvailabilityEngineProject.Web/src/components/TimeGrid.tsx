import { useRef } from 'react';
import type { PersonResponse, AttendeeBusyResponse } from '@/api/calendarApi';
import type { BusyIntervalDto } from '@/api/calendarApi';
import { useIsMobile } from '@/hooks/use-mobile';

function formatLocalTime(iso: string): string {
  const d = new Date(iso);
  return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
}

function formatLocalDate(iso: string): string {
  const d = new Date(iso);
  const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  return `${days[d.getDay()]} ${d.getDate()}`;
}

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

function addCalendarMonths(ms: number, months: number): number {
  const d = new Date(ms);
  d.setMonth(d.getMonth() + months);
  return d.getTime();
}

function addCalendarYears(ms: number, years: number): number {
  const d = new Date(ms);
  d.setFullYear(d.getFullYear() + years);
  return d.getTime();
}

const ROW_HEIGHT = 52;
const LABEL_WIDTH_DESKTOP = 160;
const LABEL_WIDTH_MOBILE = 110;
const COLUMN_COUNT = 12;
const HOUR_WIDTH_PX = 56;
const DAY_WIDTH_PX = 80;
const WEEK_WIDTH_PX = 120;
const MONTH_WIDTH_PX = 100;
const YEAR_WIDTH_PX = 90;

export type TimeGridScale = 'hours' | 'days' | 'weeks' | 'months' | 'years';

export interface TimeGridProps {
  attendees: PersonResponse[];
  windowStart: string;
  windowEnd: string;
  busyData: AttendeeBusyResponse[] | null;
  foundSlot?: { start: string; end: string } | null;
  scale?: TimeGridScale;
  editableEmail?: string;
  onEditableIntervalsChange?: (intervals: BusyIntervalDto[]) => void;
}

export function TimeGrid({
  attendees,
  windowStart,
  windowEnd,
  busyData,
  foundSlot = null,
  scale = 'hours',
}: TimeGridProps) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const isMobile = useIsMobile();
  const labelWidth = isMobile ? LABEL_WIDTH_MOBILE : LABEL_WIDTH_DESKTOP;
  const start = new Date(windowStart).getTime();
  const end = new Date(windowEnd).getTime();
  const columnCount = COLUMN_COUNT;

  const displayStart = start;
  const computedEnd =
    scale === 'hours' ? start + columnCount * 60 * 60 * 1000
    : scale === 'days' ? start + columnCount * 24 * 60 * 60 * 1000
    : scale === 'weeks' ? start + columnCount * 7 * 24 * 60 * 60 * 1000
    : scale === 'months' ? addCalendarMonths(start, columnCount)
    : addCalendarYears(start, columnCount);
  const displayEnd = Math.min(computedEnd, end);

  const displayDurationMs = displayEnd - displayStart;
  const columnDurationMs = displayDurationMs / columnCount;
  const toPercent = (t: number) => ((t - displayStart) / displayDurationMs) * 100;

  const columnStartMs = (index: number) => displayStart + index * columnDurationMs;
  const columnEndMs = (index: number) => displayStart + (index + 1) * columnDurationMs;

  const widthPx =
    scale === 'hours' ? columnCount * HOUR_WIDTH_PX
    : scale === 'days' ? columnCount * DAY_WIDTH_PX
    : scale === 'weeks' ? columnCount * WEEK_WIDTH_PX
    : scale === 'months' ? columnCount * MONTH_WIDTH_PX
    : columnCount * YEAR_WIDTH_PX;
  const columnWidth = widthPx / columnCount;

  const nowMs = Date.now();
  const nowInWindow = nowMs >= displayStart && nowMs <= displayEnd;
  const slotStart = foundSlot ? new Date(foundSlot.start).getTime() : 0;
  const slotEnd = foundSlot ? new Date(foundSlot.end).getTime() : 0;

  const nowLineLeftPx = labelWidth + (toPercent(nowMs) / 100) * widthPx;

  const getColumnLabel = (index: number) => {
    const t = new Date(columnStartMs(index));
    if (scale === 'hours') {
      return `${t.getHours().toString().padStart(2, '0')}:${t.getMinutes().toString().padStart(2, '0')}`;
    }
    if (scale === 'days') {
      return formatLocalDate(t.toISOString());
    }
    if (scale === 'weeks') {
      const weekEnd = new Date(columnEndMs(index) - 1);
      return `${formatLocalDate(t.toISOString())} – ${weekEnd.getDate()}`;
    }
    if (scale === 'months') {
      return `${MONTH_NAMES[t.getMonth()]} ${t.getFullYear()}`;
    }
    return String(t.getFullYear());
  };

  const isNowColumn = (index: number) => {
    const colStart = columnStartMs(index);
    const colEnd = columnEndMs(index);
    return nowInWindow && nowMs >= colStart && nowMs < colEnd;
  };

  return (
    <div
      ref={scrollRef}
      className="relative overflow-x-hidden overflow-y-hidden rounded-lg border border-border bg-card text-card-foreground shadow-sm w-full max-w-full pb-4"
      role="img"
      aria-label="Calendar grid showing busy blocks and availability"
    >
      <div className="relative min-w-0" style={{ width: widthPx + labelWidth }}>
        <div className="flex border-b border-border bg-muted/40 text-sm font-medium text-muted-foreground sticky top-0 z-10">
          <div
            className="shrink-0 py-3 pr-2 border-r border-border font-medium text-foreground/80 truncate sticky left-0 z-10 bg-muted/40"
            style={{ width: labelWidth }}
          >
            Attendee
          </div>
          <div className="relative flex" style={{ width: widthPx }}>
            {Array.from({ length: columnCount }, (_, i) => (
              <div
                key={i}
                className="shrink-0 border-r border-border/80 py-2 px-0.5 text-center tabular-nums text-xs"
                style={{ width: columnWidth }}
              >
                {getColumnLabel(i)}
                {isNowColumn(i) && (
                  <span className="block mt-0.5 text-[10px] font-semibold text-primary uppercase tracking-wide">
                    Now
                  </span>
                )}
              </div>
            ))}
          </div>
        </div>
        {attendees.map((p, idx) => {
          const data = busyData?.find((b) => b.email === p.email);
          const blocks = data?.busy ?? [];
          const hue = 220 + (idx * 42) % 80;

          return (
            <div
              key={p.email}
              className="flex border-b border-border/60 items-center text-sm last:border-b-0 hover:bg-muted/30 transition-colors"
              style={{ height: ROW_HEIGHT }}
            >
              <div
                className="shrink-0 pr-2 border-r border-border/60 truncate py-2 text-muted-foreground sticky left-0 z-[1] bg-card hover:bg-muted/30"
                style={{ width: labelWidth }}
                title={`${p.name} (${p.email})`}
              >
                <span className="font-medium text-foreground/90 text-base">{p.name}</span>
                <span className="text-muted-foreground text-sm"> ({p.email})</span>
              </div>
              <div
                className="relative flex-1 bg-muted/10"
                style={{ height: ROW_HEIGHT - 2, width: widthPx }}
              >
                {blocks.map((b, i) => {
                  const bStart = new Date(b.start).getTime();
                  const bEnd = new Date(b.end).getTime();
                  const clipStart = Math.max(bStart, displayStart);
                  const clipEnd = Math.min(bEnd, displayEnd);
                  if (clipEnd <= clipStart) return null;
                  const left = toPercent(clipStart);
                  const w = Math.max(1, ((clipEnd - clipStart) / (displayEnd - displayStart)) * 100);
                  return (
                    <div
                      key={i}
                      className="timegrid-busy absolute top-0.5 bottom-0.5 rounded left-0 border-l-2"
                      style={{
                        left: `${left}%`,
                        width: `${w}%`,
                        ['--busy-hue' as string]: hue,
                      }}
                      title={`Busy: ${formatLocalTime(b.start)} – ${formatLocalTime(b.end)} (local)`}
                      role="img"
                      aria-label={`Busy from ${b.start} to ${b.end}`}
                    />
                  );
                })}
              </div>
            </div>
          );
        })}
        {foundSlot && slotStart < displayEnd && slotEnd > displayStart && (
          <div className="flex border-b border-border/60 items-center text-sm bg-muted/5" style={{ height: ROW_HEIGHT }}>
            <div
              className="shrink-0 pr-2 border-r border-border/60 font-medium text-emerald-700 dark:text-emerald-400 truncate sticky left-0 z-[1] bg-muted/5"
              style={{ width: labelWidth }}
            >
              Found slot
            </div>
            <div
              className="relative flex-1"
              style={{ height: ROW_HEIGHT - 2, width: widthPx }}
            >
              <div
                className="absolute top-0.5 bottom-0.5 rounded-md border-2 border-emerald-500 bg-emerald-500/25 shadow-sm"
                style={{
                  left: `${toPercent(Math.max(slotStart, displayStart))}%`,
                  width: `${((Math.min(slotEnd, displayEnd) - Math.max(slotStart, displayStart)) / (displayEnd - displayStart)) * 100}%`,
                }}
                title={`${formatLocalTime(foundSlot.start)} – ${formatLocalTime(foundSlot.end)} (local)`}
                role="img"
                aria-label={`Available slot from ${foundSlot.start} to ${foundSlot.end}`}
              />
            </div>
          </div>
        )}
        {nowInWindow && (
          <div
            className="pointer-events-none absolute top-0 left-0 bottom-0 z-[2] w-0.5 bg-primary"
            style={{
              top: ROW_HEIGHT,
              left: nowLineLeftPx - 1,
            }}
            aria-hidden
          />
        )}
      </div>
    </div>
  );
}
