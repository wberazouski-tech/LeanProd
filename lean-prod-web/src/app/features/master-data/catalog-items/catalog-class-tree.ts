import { CatalogItemClassSummary } from '../master-data.models';

export type CatalogClassTreeItem = CatalogItemClassSummary & { depth: number };

export function buildCatalogClassTree(source: CatalogItemClassSummary[]): CatalogClassTreeItem[] {
  const result: CatalogClassTreeItem[] = [];
  const ids = new Set(source.map(item => item.id));
  const children = new Map<string | null, CatalogItemClassSummary[]>();
  for (const item of source) {
    const parent = item.parentId && ids.has(item.parentId) ? item.parentId : null;
    children.set(parent, [...(children.get(parent) ?? []), item]);
  }
  const add = (parent: string | null, depth: number): void => {
    for (const item of (children.get(parent) ?? []).sort((a, b) => a.code.localeCompare(b.code))) {
      result.push({ ...item, depth });
      add(item.id, depth + 1);
    }
  };
  add(null, 0);
  return result;
}

export function descendantLeafIds(source: CatalogItemClassSummary[], id: string): Set<string> {
  const result = new Set<string>();
  const add = (parentId: string): void => {
    for (const item of source.filter(candidate => candidate.parentId === parentId)) {
      if (!item.isGroup) result.add(item.id);
      add(item.id);
    }
  };
  add(id);
  return result;
}
