require 'eth'
require 'forwardable'

client = Eth::Client.create('http://127.0.0.1:8545')
proposer_account = Eth::Key.new priv: '0x4155bfcffbe7900c3ee6f997fb568104eb55b1fcef1bf4d51d58765b76892f77'
client.max_fee_per_gas = 41_000_000_000

# Load the contract
contract = Eth::Contract.from_file(file: '../contracts/TargetContract.sol')

# Define the constructor arguments
proposer_address = proposer_account.address
requester_address = '0x7C5687bC9AA9F12f85A3b0CBF3552086185aE794'


constructor_params = [proposer_address, requester_address]

begin
  deploy_tx_hash = client.deploy_and_wait(contract, *constructor_params, sender_key: proposer_account, gas_limit: 5_000_000)
  puts "Deployment transaction hash: #{deploy_tx_hash}"

  # Wait for the transaction receipt
  response = client.eth_get_transaction_receipt(deploy_tx_hash)
  while response.nil?
    sleep 5
    response = client.eth_get_transaction_receipt(deploy_tx_hash)
  end

  # Output the transaction receipt
  puts "Transaction receipt: #{response}"
rescue StandardError => e
  puts "An error occurred: #{e.message}"
end
