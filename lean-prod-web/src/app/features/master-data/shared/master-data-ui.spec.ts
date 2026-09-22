import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MasterDataUiModule } from './master-data-ui.module';

@Component({ standalone: true, imports: [MasterDataUiModule], template: `
  <button id="opener" (click)="open = true">Open</button>
  <div appKeyboardRow (click)="selected = true">Record</div>
  @if (open) { <dialog appModal (appModalClosed)="open = false"><h2>Editor</h2><input><button>Save</button></dialog> }
` })
class HostComponent { open = false; selected = false; }

describe('Master-data keyboard and dialogs', () => {
  it('selects a row with Enter and restores focus after modal cancellation', async () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.autoDetectChanges();
    const host: HTMLElement = fixture.nativeElement;
    host.querySelector('div')!.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    expect(fixture.componentInstance.selected).toBeTrue();
    const opener = host.querySelector<HTMLButtonElement>('#opener')!;
    opener.focus();
    fixture.ngZone!.run(() => opener.click());
    await fixture.whenStable();
    const dialog = host.querySelector('dialog')!;
    expect(dialog.open).toBeTrue();
    expect(dialog.getAttribute('aria-labelledby')).toBeTruthy();
    fixture.ngZone!.run(() => dialog.dispatchEvent(new Event('cancel', { cancelable: true })));
    await fixture.whenStable();
    expect(fixture.componentInstance.open).toBeFalse();
    expect(document.activeElement).toBe(opener);
    fixture.destroy();
  });
});
