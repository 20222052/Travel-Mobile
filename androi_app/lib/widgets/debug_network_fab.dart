// lib/widgets/debug_network_fab.dart
import 'package:flutter/material.dart';
import '../config/api_config.dart';
import '../utils/network_debug.dart';

/// Floating Action Button để debug network - CHỈ HIỂN THỊ Ở CHẾ ĐỘ DEBUG
class DebugNetworkFAB extends StatelessWidget {
  const DebugNetworkFAB({super.key});

  @override
  Widget build(BuildContext context) {
    // Chỉ hiển thị trong debug mode
    if (const bool.fromEnvironment('dart.vm.product')) {
      return const SizedBox.shrink();
    }

    return FloatingActionButton(
      mini: true,
      backgroundColor: Colors.orange.withOpacity(0.8),
      onPressed: () => _showDebugDialog(context),
      child: const Icon(Icons.network_check, size: 20),
    );
  }

  void _showDebugDialog(BuildContext context) async {
    // Chạy test kết nối
    final result = await NetworkDebug.testConnection();
    
    if (!context.mounted) return;

    final success = result['success'] as bool;
    final color = success ? Colors.green : Colors.red;
    final icon = success ? Icons.check_circle : Icons.error;

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Row(
          children: [
            Icon(icon, color: color),
            const SizedBox(width: 8),
            Text(
              success ? 'Connected' : 'Failed',
              style: TextStyle(color: color),
            ),
          ],
        ),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildInfoRow('Base URL', ApiConfig.baseUrl),
              _buildInfoRow('Is Emulator', ApiConfig.isEmulator ? 'Yes' : 'No'),
              const Divider(),
              if (success) ...[
                _buildInfoRow('Status', result['statusCode'].toString()),
                _buildInfoRow('Time', result['responseTime']),
                _buildInfoRow('Size', '${result['contentLength']} bytes'),
              ] else ...[
                _buildInfoRow('Error', result['error']),
                if (result['suggestion'] != null) ...[
                  const SizedBox(height: 8),
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: Colors.orange.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(4),
                      border: Border.all(color: Colors.orange),
                    ),
                    child: Text(
                      result['suggestion'],
                      style: const TextStyle(fontSize: 12),
                    ),
                  ),
                ],
              ],
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Close'),
          ),
          if (!success)
            TextButton(
              onPressed: () {
                Navigator.pop(context);
                _showDebugDialog(context);
              },
              child: const Text('Retry'),
            ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 80,
            child: Text(
              '$label:',
              style: const TextStyle(
                fontWeight: FontWeight.bold,
                fontSize: 12,
              ),
            ),
          ),
          Expanded(
            child: SelectableText(
              value,
              style: const TextStyle(fontSize: 12, fontFamily: 'monospace'),
            ),
          ),
        ],
      ),
    );
  }
}
