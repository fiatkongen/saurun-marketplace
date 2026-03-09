// ============================================================
// api.ts — Mock API layer for the Activity Dashboard
// ============================================================

import type {
  ActivityFeedResponse,
  ActivityItem,
  ActivityType,
  DateRange,
  StatCardData,
  StatsResponse,
} from "./types";

// ---------------------------------------------------------------------------
// Mock data generators
// ---------------------------------------------------------------------------

const MOCK_USERS = [
  { name: "Alice Johnson", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=alice", email: "alice@example.com" },
  { name: "Bob Smith", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=bob", email: "bob@example.com" },
  { name: "Carol White", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=carol", email: "carol@example.com" },
  { name: "Dan Brown", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=dan", email: "dan@example.com" },
  { name: "Eva Martinez", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=eva", email: "eva@example.com" },
  { name: "Frank Lee", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=frank", email: "frank@example.com" },
  { name: "Grace Kim", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=grace", email: "grace@example.com" },
  { name: "Henry Chen", avatarUrl: "https://api.dicebear.com/9.x/avataaars/svg?seed=henry", email: "henry@example.com" },
];

const ACTIVITY_TYPES: ActivityType[] = ["login", "purchase", "signup"];

const DESCRIPTIONS: Record<ActivityType, string[]> = {
  login: ["Logged in from Chrome on macOS", "Logged in from Safari on iOS", "Logged in via SSO"],
  purchase: ["Purchased Pro Plan ($29/mo)", "Purchased Team Plan ($99/mo)", "Bought 100 credits ($10)"],
  signup: ["Signed up via Google OAuth", "Created account with email", "Signed up via GitHub"],
};

function randomItem<T>(arr: T[]): T {
  return arr[Math.floor(Math.random() * arr.length)];
}

function generateActivity(id: number, baseTimestamp: number): ActivityItem {
  const type = randomItem(ACTIVITY_TYPES);
  const user = randomItem(MOCK_USERS);
  const description = randomItem(DESCRIPTIONS[type]);
  // Each item is 1-30 minutes apart
  const offset = id * (Math.floor(Math.random() * 29) + 1) * 60 * 1000;
  const timestamp = new Date(baseTimestamp - offset).toISOString();

  return {
    id: `activity-${id}`,
    type,
    user,
    description,
    timestamp,
  };
}

// ---------------------------------------------------------------------------
// Simulated delay
// ---------------------------------------------------------------------------

function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

// ---------------------------------------------------------------------------
// API functions
// ---------------------------------------------------------------------------

export async function fetchStats(): Promise<StatsResponse> {
  await delay(800);

  const stats: StatCardData[] = [
    {
      id: "total-users",
      label: "Total Users",
      value: "24,521",
      trend: { direction: "up", percentage: 12.5 },
      icon: "users",
    },
    {
      id: "revenue",
      label: "Revenue",
      value: "$48,290",
      trend: { direction: "up", percentage: 8.2 },
      icon: "dollar-sign",
    },
    {
      id: "active-sessions",
      label: "Active Sessions",
      value: "1,429",
      trend: { direction: "down", percentage: 3.1 },
      icon: "activity",
    },
    {
      id: "conversion-rate",
      label: "Conversion Rate",
      value: "3.24%",
      trend: { direction: "up", percentage: 1.8 },
      icon: "target",
    },
  ];

  return { stats };
}

export async function fetchActivityFeed(params: {
  cursor?: string | null;
  pageSize?: number;
  activityTypes?: ActivityType[];
  dateRange?: DateRange;
}): Promise<ActivityFeedResponse> {
  await delay(600);

  const { cursor, pageSize = 20, activityTypes } = params;
  const cursorNum = cursor ? parseInt(cursor, 10) : 0;
  const baseTimestamp = Date.now();

  let items: ActivityItem[] = [];
  for (let i = 0; i < pageSize + 5; i++) {
    items.push(generateActivity(cursorNum + i, baseTimestamp));
  }

  // Filter by activity type if specified
  if (activityTypes && activityTypes.length > 0) {
    items = items.filter((item) => activityTypes.includes(item.type));
  }

  // Trim to page size
  items = items.slice(0, pageSize);

  // Simulate end of feed after ~200 items
  const nextCursorNum = cursorNum + pageSize;
  const hasMore = nextCursorNum < 200;

  return {
    items,
    nextCursor: hasMore ? String(nextCursorNum) : null,
    totalCount: 200,
  };
}
