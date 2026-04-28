import { Component, OnInit, OnDestroy, ViewChild, ElementRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { DespesaService } from '../../core/services/despesa.service';
import { Dashboard, EvolucaoMensalItem, RankingItem } from '../../core/models/despesa.model';

Chart.register(...registerables);

@Component({
  selector: 'app-painel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './painel.component.html',
  styleUrl: './painel.component.scss'
})
export class PainelComponent implements OnInit, OnDestroy {
  @ViewChild('canvasBarra')  canvasBarra!:  ElementRef<HTMLCanvasElement>;
  @ViewChild('canvasDonut')  canvasDonut!:  ElementRef<HTMLCanvasElement>;

  readonly carregando = signal(false);
  readonly dados      = signal<Dashboard | null>(null);

  de  = this.primeiroDiaMes();
  ate = this.hoje();

  private graficoBarra: Chart | null = null;
  private graficoDonut: Chart | null = null;

  constructor(private service: DespesaService) {}

  ngOnInit(): void {
    this.carregar();
  }

  ngOnDestroy(): void {
    this.graficoBarra?.destroy();
    this.graficoDonut?.destroy();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.obterDashboard(this.de, this.ate).subscribe({
      next: d => {
        this.dados.set(d);
        this.carregando.set(false);
        setTimeout(() => this.renderizarGraficos());
      },
      error: () => this.carregando.set(false)
    });
  }

  private renderizarGraficos(): void {
    const d = this.dados();
    if (!d) return;

    this.renderizarBarra(d.evolucaoMensal);
    this.renderizarDonut(d.resumo.totalEssencial, d.resumo.totalLazer, d.resumo.totalInvestimento);
  }

  private renderizarBarra(evolucao: EvolucaoMensalItem[]): void {
    this.graficoBarra?.destroy();

    const meses = this.gerarMeses();
    const labels = meses.map(m => m.label);
    const mapDados = new Map(evolucao.map(e => [`${e.ano}-${e.mes}`, e]));

    const essencial    = meses.map(m => Number(mapDados.get(`${m.ano}-${m.mes}`)?.totalEssencial    ?? 0));
    const lazer        = meses.map(m => Number(mapDados.get(`${m.ano}-${m.mes}`)?.totalLazer        ?? 0));
    const investimento = meses.map(m => Number(mapDados.get(`${m.ano}-${m.mes}`)?.totalInvestimento ?? 0));

    this.graficoBarra = new Chart(this.canvasBarra.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          { label: 'Essencial',    data: essencial,    backgroundColor: '#86efac', borderColor: '#22c55e', borderWidth: 1 },
          { label: 'Lazer',        data: lazer,        backgroundColor: '#93c5fd', borderColor: '#3b82f6', borderWidth: 1 },
          { label: 'Investimento', data: investimento, backgroundColor: '#fde68a', borderColor: '#f59e0b', borderWidth: 1 },
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: 'top' } },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: v => 'R$ ' + Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 0 })
            }
          }
        }
      }
    });
  }

  private renderizarDonut(essencial: number, lazer: number, investimento: number): void {
    this.graficoDonut?.destroy();

    this.graficoDonut = new Chart(this.canvasDonut.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Essencial', 'Lazer', 'Investimento'],
        datasets: [{
          data: [Number(essencial), Number(lazer), Number(investimento)],
          backgroundColor: ['#22c55e', '#3b82f6', '#f59e0b'],
          borderWidth: 2,
          borderColor: '#fff'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom' },
          tooltip: {
            callbacks: {
              label: ctx => {
                const val = ctx.parsed;
                const total = (ctx.dataset.data as number[]).reduce((a, b) => a + b, 0);
                const pct = total > 0 ? ((val / total) * 100).toFixed(1) : '0';
                return ` ${ctx.label}: R$ ${val.toLocaleString('pt-BR', { minimumFractionDigits: 2 })} (${pct}%)`;
              }
            }
          }
        }
      }
    });
  }

  private gerarMeses(): { ano: number; mes: number; label: string }[] {
    const inicio = new Date(this.de + 'T00:00:00');
    const fim    = new Date(this.ate + 'T00:00:00');
    const atual  = new Date(inicio.getFullYear(), inicio.getMonth(), 1);
    const result: { ano: number; mes: number; label: string }[] = [];

    while (atual <= fim) {
      result.push({
        ano:   atual.getFullYear(),
        mes:   atual.getMonth() + 1,
        label: atual.toLocaleDateString('pt-BR', { month: 'short', year: '2-digit' })
      });
      atual.setMonth(atual.getMonth() + 1);
    }
    return result;
  }

  formatarValor(v: number): string {
    return Number(v).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  porcentagem(parcial: number, total: number): string {
    if (!total) return '0%';
    return ((Number(parcial) / Number(total)) * 100).toFixed(1) + '%';
  }

  private primeiroDiaMes(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`;
  }

  private hoje(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
